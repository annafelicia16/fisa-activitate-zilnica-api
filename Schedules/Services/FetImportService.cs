using System.Collections.Frozen;
using FisaActivitateZilnicaApi.ExternalReferences.Models;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Services.FetParser;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

namespace FisaActivitateZilnicaApi.Schedules.Services;

public class FetImportService(
    ISchedulesRepository schedulesRepo,
    IExternalReferencesRepository externalReferencesRepository
) : IFetImportService
{
    public async Task<FetImportResult> ImportAsync(
        Stream oddWeekFetStream,
        Stream evenWeekFetStream,
        string name,
        int year,
        int semester,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            return Failure("Schedule name is required.");

        if (year <= 0)
            return Failure("Year must be greater than 0.");

        if (semester <= 0)
            return Failure("Semester must be greater than 0.");

        if (startDate.Date > endDate.Date)
            return Failure("'startDate' cannot be greater than 'endDate'.");

        ScheduleYear scheduleYear = await GetOrCreateScheduleYearAsync(year, ct);
        ScheduleSemester? existingScheduleSemester =
            await schedulesRepo.GetScheduleSemesterByYearAndNumberAsync(
                scheduleYear.Id,
                semester,
                ct
            );

        ScheduleSemester scheduleSemester;
        if (existingScheduleSemester == null)
        {
            scheduleSemester = new ScheduleSemester
            {
                ScheduleYearId = scheduleYear.Id,
                Number = semester,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
            };
            await schedulesRepo.AddScheduleSemesterAsync(scheduleSemester, ct);
        }
        else
        {
            scheduleSemester = existingScheduleSemester;

            if (
                scheduleSemester.StartDate.Date != startDate.Date
                || scheduleSemester.EndDate.Date != endDate.Date
            )
            {
                return new FetImportResult(
                    false,
                    scheduleYear.Id,
                    scheduleSemester.Id,
                    null,
                    null,
                    "A schedule semester already exists with different start or end dates."
                );
            }
        }

        if (
            await schedulesRepo.AnyScheduleAsync(
                schedule => schedule.ScheduleSemesterId == scheduleSemester.Id && schedule.OddWeek,
                ct
            )
        )
        {
            return new FetImportResult(
                false,
                scheduleYear.Id,
                scheduleSemester.Id,
                null,
                null,
                "An odd week schedule already exists for this year and semester."
            );
        }

        if (
            await schedulesRepo.AnyScheduleAsync(
                schedule => schedule.ScheduleSemesterId == scheduleSemester.Id && !schedule.OddWeek,
                ct
            )
        )
        {
            return new FetImportResult(
                false,
                scheduleYear.Id,
                scheduleSemester.Id,
                null,
                null,
                "An even week schedule already exists for this year and semester."
            );
        }

        int oddWeekScheduleId = await ImportSingleScheduleAsync(
            oddWeekFetStream,
            name,
            scheduleSemester.Id,
            true,
            ct
        );
        int evenWeekScheduleId = await ImportSingleScheduleAsync(
            evenWeekFetStream,
            name,
            scheduleSemester.Id,
            false,
            ct
        );

        return new FetImportResult(
            true,
            scheduleYear.Id,
            scheduleSemester.Id,
            oddWeekScheduleId,
            evenWeekScheduleId,
            null
        );
    }

    private async Task<ScheduleYear> GetOrCreateScheduleYearAsync(int year, CancellationToken ct)
    {
        ScheduleYear? existingScheduleYear = await schedulesRepo.GetScheduleYearByValueAsync(
            year,
            ct
        );
        if (existingScheduleYear != null)
            return existingScheduleYear;

        ScheduleYear scheduleYear = new() { Value = year };
        await schedulesRepo.AddScheduleYearAsync(scheduleYear, ct);
        return scheduleYear;
    }

    private async Task<int> ImportSingleScheduleAsync(
        Stream fetStream,
        string name,
        int scheduleSemesterId,
        bool oddWeek,
        CancellationToken ct
    )
    {
        FetParsedData data = FetXmlParser.Parse(fetStream);

        Schedule schedule = new()
        {
            Name = name.Trim(),
            ScheduleSemesterId = scheduleSemesterId,
            OddWeek = oddWeek,
        };
        await schedulesRepo.AddScheduleAsync(schedule, ct);

        int scheduleId = schedule.Id;

        List<Day> days = data
            .Days.Select(day => new Day { Name = day.Name, ScheduleId = scheduleId })
            .ToList();
        await schedulesRepo.AddDaysAsync(days, ct);

        List<Hour> hours = data
            .Hours.Select(hour => new Hour { Name = hour.Name, ScheduleId = scheduleId })
            .ToList();
        await schedulesRepo.AddHoursAsync(hours, ct);

        List<Subject> subjects = data
            .Subjects.Select(subject => new Subject
            {
                Name = subject.Name,
                Comments = subject.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddSubjectsAsync(subjects, ct);

        FrozenDictionary<string, Subject> subjectByName = subjects.ToFrozenDictionary(
            subject => subject.Name,
            StringComparer.OrdinalIgnoreCase
        );
        Dictionary<int, Subject> subjectByExternalId = subjects
            .Where(subject => subject.ExternalSubjectId.HasValue)
            .ToDictionary(subject => subject.ExternalSubjectId!.Value, subject => subject);

        List<ActivityTag> activityTags = data
            .ActivityTags.Select(tag => new ActivityTag
            {
                Name = tag.Name,
                Printable = tag.Printable,
                Comments = tag.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddActivityTagsAsync(activityTags, ct);

        FrozenDictionary<string, ActivityTag> tagByName = activityTags.ToFrozenDictionary(
            tag => tag.Name,
            StringComparer.OrdinalIgnoreCase
        );

        List<Teacher> teachers = data
            .Teachers.Select(teacher => new Teacher
            {
                Name = teacher.Name,
                TargetNumberOfHours = teacher.TargetNumberOfHours,
                Comments = teacher.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddTeachersAsync(teachers, ct);

        FrozenDictionary<string, Teacher> teacherByName = teachers.ToFrozenDictionary(
            teacher => teacher.Name,
            StringComparer.OrdinalIgnoreCase
        );
        Dictionary<int, Teacher> teacherByExternalId = teachers
            .Where(teacher => teacher.ExternalTeacherId.HasValue)
            .ToDictionary(teacher => teacher.ExternalTeacherId!.Value, teacher => teacher);

        List<Building> buildings = data
            .Buildings.Select(building => new Building
            {
                Name = building.Name,
                Comments = building.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddBuildingsAsync(buildings, ct);

        FrozenDictionary<string, Building> buildingByName = buildings
            .Where(building => !string.IsNullOrWhiteSpace(building.Name))
            .ToFrozenDictionary(building => building.Name, StringComparer.OrdinalIgnoreCase);

        List<Room> rooms = [];
        foreach (var room in data.Rooms)
        {
            int? buildingId = null;
            if (
                !string.IsNullOrWhiteSpace(room.BuildingName)
                && buildingByName.TryGetValue(room.BuildingName, out Building? building)
            )
            {
                buildingId = building.Id;
            }

            rooms.Add(
                new Room
                {
                    Name = room.Name,
                    BuildingId = buildingId,
                    Capacity = room.Capacity,
                    Virtual = room.Virtual,
                    Comments = room.Comments,
                    ScheduleId = scheduleId,
                }
            );
        }
        await schedulesRepo.AddRoomsAsync(rooms, ct);

        // Resolve which Room each Activity is bound to via Space_Constraints_List /
        // ConstraintActivityPreferredRoom. FET stores this binding per-activity (not per
        // slot), so every slot for a given activity uses the same room.
        FrozenDictionary<string, Room> roomByName = rooms
            .Where(room => !string.IsNullOrWhiteSpace(room.Name))
            .ToFrozenDictionary(room => room.Name, StringComparer.OrdinalIgnoreCase);
        Dictionary<int, int> roomIdByFetActivityId = new();
        foreach (var activityRoom in data.ActivityRooms)
        {
            if (roomByName.TryGetValue(activityRoom.RoomName, out Room? matchedRoom))
            {
                roomIdByFetActivityId[activityRoom.FetActivityId] = matchedRoom.Id;
            }
        }

        FrozenDictionary<string, Day> dayByName = days.ToFrozenDictionary(
            day => day.Name,
            StringComparer.OrdinalIgnoreCase
        );
        FrozenDictionary<string, Hour> hourByName = hours.ToFrozenDictionary(
            hour => hour.Name,
            StringComparer.OrdinalIgnoreCase
        );
        FrozenDictionary<int, FetActivity> fetActivityByFetId = data
            .Activities.Where(activity => activity.FetId >= 0)
            .ToFrozenDictionary(activity => activity.FetId, activity => activity);

        foreach (var scheduleYear in data.Years)
        {
            Year yearEntity = new()
            {
                Name = scheduleYear.Name,
                NumberOfStudents = scheduleYear.NumberOfStudents,
                Comments = scheduleYear.Comments,
                ScheduleId = scheduleId,
            };
            await schedulesRepo.AddYearAsync(yearEntity, ct);

            foreach (var group in scheduleYear.Groups)
            {
                Group groupEntity = new()
                {
                    Name = group.Name,
                    NumberOfStudents = group.NumberOfStudents,
                    Comments = group.Comments,
                    YearId = yearEntity.Id,
                    Year = yearEntity,
                };
                await schedulesRepo.AddGroupAsync(groupEntity, ct);

                foreach (var subgroup in group.Subgroups)
                {
                    await schedulesRepo.AddSubgroupAsync(
                        new Subgroup
                        {
                            Name = subgroup.Name,
                            NumberOfStudents = subgroup.NumberOfStudents,
                            Comments = subgroup.Comments,
                            GroupId = groupEntity.Id,
                            Group = groupEntity,
                        },
                        ct
                    );
                }
            }
        }

        // Resolve AGSIS friendly names once for the whole file (sequential — the
        // University DbContext can't run concurrent queries), then stamp them onto
        // each ActivityStudents row below. Mirrors the batching in
        // SchedulesQueryService.GetTeacherDaySlotsByDateAsync.
        List<FetActivityCommentRef> allCommentRefs = data
            .Activities.SelectMany(activity => activity.CommentRefs)
            .ToList();

        int[] subjectExternalIds = allCommentRefs
            .Where(r => r.SubjectExternalId is > 0)
            .Select(r => r.SubjectExternalId!.Value)
            .Distinct()
            .ToArray();
        int[] facultyExternalIds = allCommentRefs
            .Where(r => r.FacultyExternalId is > 0)
            .Select(r => r.FacultyExternalId!.Value)
            .Distinct()
            .ToArray();
        int[] groupExternalIds = allCommentRefs
            .Select(r => SpecializationResolver.TryParseFirstInt(r.GroupExternalId))
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        Dictionary<int, ExternalSubject> subjectsById = (
            await externalReferencesRepository.GetSubjectsByIdsAsync(subjectExternalIds, ct)
        ).ToDictionary(s => (int)s.IdMaterie);
        Dictionary<int, ExternalFaculty> facultiesById = (
            await externalReferencesRepository.GetFacultiesByIdsAsync(facultyExternalIds, ct)
        ).ToDictionary(f => (int)f.IdFacultate);
        Dictionary<int, ExternalGroup> groupsById = (
            await externalReferencesRepository.GetGroupsByIdsAsync(groupExternalIds, ct)
        ).ToDictionary(g => (int)g.IdGrupe);

        // Short-name + faculty hint for rows lacking both an own specialization and a
        // resolvable group id (e.g. course rows tagged "IE3").
        var shortNameFacultyPairs = allCommentRefs
            .Where(r =>
                !(r.SpecializationExternalId is > 0)
                && SpecializationResolver.TryParseFirstInt(r.GroupExternalId) <= 0
                && r.FacultyExternalId is > 0
            )
            .Select(r =>
                (
                    Prefix: SpecializationResolver.ExtractAlphaPrefix(r.GroupNames),
                    FacultyId: r.FacultyExternalId!.Value
                )
            )
            .Where(p => !string.IsNullOrEmpty(p.Prefix))
            .Distinct()
            .ToArray();
        string[] shortNames = shortNameFacultyPairs.Select(p => p.Prefix!).Distinct().ToArray();
        int[] shortNameFacultyIds = shortNameFacultyPairs
            .Select(p => p.FacultyId)
            .Distinct()
            .ToArray();

        Dictionary<(string ShortName, int FacultyId), int> shortNameToSpecId = (
            await externalReferencesRepository.GetSpecializationsByShortNameAndFacultiesAsync(
                shortNames,
                shortNameFacultyIds,
                ct
            )
        )
            .GroupBy(l => (
                ShortName: l.DenumireScurta!.ToUpperInvariant(),
                FacultyId: (int)l.FacultyId
            ))
            .ToDictionary(g => g.Key, g => (int)g.First().IdSpecializare);

        int[] effectiveSpecExternalIds = allCommentRefs
            .Select(r =>
                SpecializationResolver.ResolveEffectiveSpecializationId(
                    r.SpecializationExternalId is > 0 ? r.SpecializationExternalId : null,
                    r.GroupExternalId,
                    r.FacultyExternalId,
                    r.GroupNames,
                    groupsById,
                    shortNameToSpecId
                )
            )
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
        Dictionary<int, ExternalSpecialization> specializationsById = (
            await externalReferencesRepository.GetSpecializationsByIdsAsync(
                effectiveSpecExternalIds,
                ct
            )
        ).ToDictionary(s => (int)s.IdSpecializare);

        Dictionary<int, Activity> fetIdToActivity = [];
        foreach (var activityData in data.Activities)
        {
            Subject? subject = ResolveSubject(
                subjectByName,
                subjectByExternalId,
                activityData.SubjectName,
                activityData.CommentRefs
            );
            if (subject == null)
                continue;

            int? activityTagId = null;
            if (
                !string.IsNullOrEmpty(activityData.ActivityTagName)
                && tagByName.TryGetValue(activityData.ActivityTagName, out ActivityTag? tag)
            )
            {
                activityTagId = tag.Id;
            }

            Activity activity = new()
            {
                ScheduleId = scheduleId,
                SubjectId = subject.Id,
                Subject = subject,
                ActivityTagId = activityTagId,
                Duration = activityData.Duration,
                TotalDuration = activityData.TotalDuration,
                ActivityGroupId = activityData.ActivityGroupId,
                Active = activityData.Active,
                Comments = activityData.Comments,
            };
            await schedulesRepo.AddActivityAsync(activity, ct);

            if (activityData.FetId >= 0)
                fetIdToActivity[activityData.FetId] = activity;

            HashSet<int> teacherIdsAdded = [];
            IReadOnlyList<string> teacherNames = SplitByPlus(activityData.TeacherName);

            foreach (var commentRef in activityData.CommentRefs)
            {
                Teacher? teacher = ResolveTeacher(
                    teacherByName,
                    teacherByExternalId,
                    teacherNames,
                    commentRef.TeacherExternalId
                );
                if (teacher == null || !teacherIdsAdded.Add(teacher.Id))
                    continue;

                await schedulesRepo.AddActivityTeacherAsync(
                    new ActivityTeacher
                    {
                        ActivityId = activity.Id,
                        TeacherId = teacher.Id,
                        Activity = activity,
                        Teacher = teacher,
                    },
                    ct
                );
            }

            foreach (var teacherName in teacherNames)
            {
                if (!teacherByName.TryGetValue(teacherName, out Teacher? teacher))
                    continue;
                if (!teacherIdsAdded.Add(teacher.Id))
                    continue;

                await schedulesRepo.AddActivityTeacherAsync(
                    new ActivityTeacher
                    {
                        ActivityId = activity.Id,
                        TeacherId = teacher.Id,
                        Activity = activity,
                        Teacher = teacher,
                    },
                    ct
                );
            }

            if (activityData.CommentRefs.Count > 0)
            {
                foreach (var commentRef in activityData.CommentRefs)
                {
                    int? specializationExternalId = commentRef.SpecializationExternalId is > 0
                        ? commentRef.SpecializationExternalId
                        : null;
                    string studentsName = !string.IsNullOrWhiteSpace(commentRef.GroupExternalId)
                        ? $"group:{commentRef.GroupExternalId}"
                        : specializationExternalId.HasValue
                            ? $"specialization:{specializationExternalId.Value}"
                            : activityData.Students;

                    if (string.IsNullOrWhiteSpace(studentsName))
                        continue;

                    int effectiveSpecId = SpecializationResolver.ResolveEffectiveSpecializationId(
                        specializationExternalId,
                        commentRef.GroupExternalId,
                        commentRef.FacultyExternalId,
                        commentRef.GroupNames,
                        groupsById,
                        shortNameToSpecId
                    );
                    int firstGroupId = SpecializationResolver.TryParseFirstInt(
                        commentRef.GroupExternalId
                    );

                    await schedulesRepo.AddActivityStudentsAsync(
                        new ActivityStudents
                        {
                            ActivityId = activity.Id,
                            StudentsName = studentsName,
                            PlanMatterProviderExternalId = commentRef.PlanMatterProviderExternalId,
                            FacultyExternalId = commentRef.FacultyExternalId,
                            MetaSpecializationExternalId =
                                commentRef.MetaSpecializationExternalId,
                            StudyYearNumber = commentRef.StudyYearNumber,
                            Semester = commentRef.Semester,
                            PlataNB = commentRef.PlataNB,
                            OldActivityTag = commentRef.OldActivityTag,
                            GroupExternalId = commentRef.GroupExternalId,
                            GroupNames = commentRef.GroupNames,
                            SpecializationExternalId = specializationExternalId,
                            SubjectExternalId = commentRef.SubjectExternalId,
                            FacultyName =
                                commentRef.FacultyExternalId is int facId
                                && facultiesById.TryGetValue(facId, out ExternalFaculty? faculty)
                                    ? faculty.Denumire
                                    : null,
                            SpecializationName =
                                effectiveSpecId > 0
                                && specializationsById.TryGetValue(
                                    effectiveSpecId,
                                    out ExternalSpecialization? specialization
                                )
                                    ? specialization.Denumire
                                    : null,
                            SubjectName =
                                commentRef.SubjectExternalId is int subjId
                                && subjectsById.TryGetValue(
                                    subjId,
                                    out ExternalSubject? agsisSubject
                                )
                                    ? agsisSubject.Denumire
                                    : null,
                            ResolvedGroupName =
                                firstGroupId > 0
                                && groupsById.TryGetValue(firstGroupId, out ExternalGroup? group)
                                    ? group.Nume
                                    : commentRef.GroupNames,
                            EffectiveSpecializationExternalId =
                                effectiveSpecId > 0 ? effectiveSpecId : null,
                            Activity = activity,
                        },
                        ct
                    );
                }
            }
            else
            {
                foreach (var studentsName in SplitByPlus(activityData.Students))
                {
                    if (string.IsNullOrWhiteSpace(studentsName))
                        continue;

                    await schedulesRepo.AddActivityStudentsAsync(
                        new ActivityStudents
                        {
                            ActivityId = activity.Id,
                            StudentsName = studentsName,
                            Activity = activity,
                        },
                        ct
                    );
                }
            }
        }

        FrozenDictionary<string, int> hourOrder = data
            .Hours.Select((hour, index) => (hour.Name, index))
            .ToFrozenDictionary(item => item.Name, item => item.index, StringComparer.OrdinalIgnoreCase);

        List<ActivitySlot> activitySlots = [];
        foreach (var slot in data.ActivitySlots)
        {
            if (!fetIdToActivity.TryGetValue(slot.FetActivityId, out Activity? activity))
                continue;
            if (!dayByName.TryGetValue(slot.DayName, out Day? day))
                continue;
            if (!hourOrder.TryGetValue(slot.HourName, out int startIndex))
                continue;

            int duration = fetActivityByFetId.TryGetValue(slot.FetActivityId, out FetActivity? fa)
                ? fa.Duration
                : 1;

            int? slotRoomId = roomIdByFetActivityId.TryGetValue(
                slot.FetActivityId,
                out int rid
            )
                ? rid
                : null;

            for (int i = 0; i < duration && startIndex + i < data.Hours.Count; i++)
            {
                string hourName = data.Hours[startIndex + i].Name;
                if (!hourByName.TryGetValue(hourName, out Hour? slotHour))
                    continue;

                activitySlots.Add(
                    new ActivitySlot
                    {
                        ScheduleId = scheduleId,
                        ActivityId = activity.Id,
                        DayId = day.Id,
                        HourId = slotHour.Id,
                        RoomId = slotRoomId,
                    }
                );
            }
        }
        await schedulesRepo.AddActivitySlotsAsync(activitySlots, ct);

        return scheduleId;
    }

    private static FetImportResult Failure(string message) =>
        new(false, null, null, null, null, message);

    private static IReadOnlyList<string> SplitByPlus(string value)
    {
        return value
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private static Subject? ResolveSubject(
        FrozenDictionary<string, Subject> subjectByName,
        Dictionary<int, Subject> subjectByExternalId,
        string subjectName,
        IReadOnlyList<FetActivityCommentRef> commentRefs
    )
    {
        foreach (var commentRef in commentRefs)
        {
            if (!commentRef.SubjectExternalId.HasValue)
                continue;

            int subjectExternalId = commentRef.SubjectExternalId.Value;
            if (subjectByExternalId.TryGetValue(subjectExternalId, out Subject? subjectById))
                return subjectById;

            if (!subjectByName.TryGetValue(subjectName, out Subject? subjectByActivityName))
                continue;

            if (
                subjectByActivityName.ExternalSubjectId.HasValue
                && subjectByActivityName.ExternalSubjectId.Value != subjectExternalId
            )
            {
                continue;
            }

            subjectByActivityName.ExternalSubjectId = subjectExternalId;
            subjectByExternalId[subjectExternalId] = subjectByActivityName;
            return subjectByActivityName;
        }

        return subjectByName.TryGetValue(subjectName, out Subject? subjectByFallbackName)
            ? subjectByFallbackName
            : null;
    }

    private static Teacher? ResolveTeacher(
        FrozenDictionary<string, Teacher> teacherByName,
        Dictionary<int, Teacher> teacherByExternalId,
        IReadOnlyList<string> teacherNames,
        int? teacherExternalId
    )
    {
        if (!teacherExternalId.HasValue)
            return null;

        if (teacherByExternalId.TryGetValue(teacherExternalId.Value, out Teacher? teacherById))
            return teacherById;

        foreach (var teacherName in teacherNames)
        {
            if (!teacherByName.TryGetValue(teacherName, out Teacher? teacherByActivityName))
                continue;

            if (
                teacherByActivityName.ExternalTeacherId.HasValue
                && teacherByActivityName.ExternalTeacherId.Value != teacherExternalId.Value
            )
            {
                continue;
            }

            teacherByActivityName.ExternalTeacherId = teacherExternalId.Value;
            teacherByExternalId[teacherExternalId.Value] = teacherByActivityName;
            return teacherByActivityName;
        }

        return null;
    }
}
