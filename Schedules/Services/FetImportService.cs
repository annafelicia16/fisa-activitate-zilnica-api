using System.Collections.Frozen;
using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Services.FetParser;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;

namespace FisaActivitateZilnicaApi.Schedules.Services;

public class FetImportService(ISchedulesRepository schedulesRepo) : IFetImportService
{
    public async Task<FetImportResult> ImportAsync(
        Stream fetStream,
        string name,
        int year,
        int semester,
        bool oddWeek,
        CancellationToken ct = default
    )
    {
        var exists = await schedulesRepo.AnyScheduleAsync(
            s => s.Year == year && s.Semester == semester && s.OddWeek == oddWeek,
            ct
        );
        if (exists)
            return new FetImportResult(
                false,
                null,
                "A schedule already exists for this year, semester, and week type."
            );

        var data = FetXmlParser.Parse(fetStream);

        var schedule = new Schedule
        {
            Name = name,
            Year = year,
            Semester = semester,
            OddWeek = oddWeek,
        };
        await schedulesRepo.AddScheduleAsync(schedule, ct);

        var scheduleId = schedule.Id;

        // Days
        var days = data
            .Days.Select(d => new Day { Name = d.Name, ScheduleId = scheduleId })
            .ToList();
        await schedulesRepo.AddDaysAsync(days, ct);

        // Hours
        var hours = data
            .Hours.Select(h => new Hour { Name = h.Name, ScheduleId = scheduleId })
            .ToList();
        await schedulesRepo.AddHoursAsync(hours, ct);

        // Subjects
        var subjects = data
            .Subjects.Select(s => new Subject
            {
                Name = s.Name,
                Comments = s.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddSubjectsAsync(subjects, ct);
        var subjectByName = subjects.ToFrozenDictionary(
            s => s.Name,
            StringComparer.OrdinalIgnoreCase
        );
        var subjectByExternalId = subjects
            .Where(s => s.ExternalSubjectId.HasValue)
            .ToDictionary(s => s.ExternalSubjectId!.Value, s => s);

        // ActivityTags
        var activityTags = data
            .ActivityTags.Select(t => new ActivityTag
            {
                Name = t.Name,
                Printable = t.Printable,
                Comments = t.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddActivityTagsAsync(activityTags, ct);
        var tagByName = activityTags.ToFrozenDictionary(
            t => t.Name,
            StringComparer.OrdinalIgnoreCase
        );

        // Teachers
        var teachers = data
            .Teachers.Select(t => new Teacher
            {
                Name = t.Name,
                TargetNumberOfHours = t.TargetNumberOfHours,
                Comments = t.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddTeachersAsync(teachers, ct);
        var teacherByName = teachers.ToFrozenDictionary(
            t => t.Name,
            StringComparer.OrdinalIgnoreCase
        );
        var teacherByExternalId = teachers
            .Where(t => t.ExternalTeacherId.HasValue)
            .ToDictionary(t => t.ExternalTeacherId!.Value, t => t);

        // Buildings
        var buildings = data
            .Buildings.Select(b => new Building
            {
                Name = b.Name,
                Comments = b.Comments,
                ScheduleId = scheduleId,
            })
            .ToList();
        await schedulesRepo.AddBuildingsAsync(buildings, ct);
        var buildingByName = buildings
            .Where(b => !string.IsNullOrWhiteSpace(b.Name))
            .ToFrozenDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);

        // Rooms (resolve Building by name)
        var rooms = new List<Room>();
        foreach (var r in data.Rooms)
        {
            int? buildingId = null;
            if (
                !string.IsNullOrWhiteSpace(r.BuildingName)
                && buildingByName.TryGetValue(r.BuildingName, out var b)
            )
                buildingId = b.Id;
            rooms.Add(
                new Room
                {
                    Name = r.Name,
                    BuildingId = buildingId,
                    Capacity = r.Capacity,
                    Virtual = r.Virtual,
                    Comments = r.Comments,
                    ScheduleId = scheduleId,
                }
            );
        }
        await schedulesRepo.AddRoomsAsync(rooms, ct);

        var dayByName = days.ToFrozenDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);
        var hourByName = hours.ToFrozenDictionary(h => h.Name, StringComparer.OrdinalIgnoreCase);
        var fetActivityByFetId = data
            .Activities.Where(x => x.FetId >= 0)
            .ToFrozenDictionary(x => x.FetId, x => x);

        // Years, Groups, Subgroups
        foreach (var y in data.Years)
        {
            var yearEntity = new Year
            {
                Name = y.Name,
                NumberOfStudents = y.NumberOfStudents,
                Comments = y.Comments,
                ScheduleId = scheduleId,
            };
            await schedulesRepo.AddYearAsync(yearEntity, ct);

            foreach (var g in y.Groups)
            {
                var groupEntity = new Group
                {
                    Name = g.Name,
                    NumberOfStudents = g.NumberOfStudents,
                    Comments = g.Comments,
                    YearId = yearEntity.Id,
                    Year = yearEntity,
                };
                await schedulesRepo.AddGroupAsync(groupEntity, ct);

                foreach (var sg in g.Subgroups)
                {
                    await schedulesRepo.AddSubgroupAsync(
                        new Subgroup
                        {
                            Name = sg.Name,
                            NumberOfStudents = sg.NumberOfStudents,
                            Comments = sg.Comments,
                            GroupId = groupEntity.Id,
                            Group = groupEntity,
                        },
                        ct
                    );
                }
            }
        }

        // Activities (build fetId -> Activity mapping for slots)
        var fetIdToActivity = new Dictionary<int, Activity>();
        foreach (var a in data.Activities)
        {
            var subject = ResolveSubject(subjectByName, subjectByExternalId, a.SubjectName, a.CommentRefs);
            if (subject == null)
                continue;

            int? activityTagId = null;
            if (
                !string.IsNullOrEmpty(a.ActivityTagName)
                && tagByName.TryGetValue(a.ActivityTagName, out var tag)
            )
                activityTagId = tag.Id;

            var activity = new Activity
            {
                ScheduleId = scheduleId,
                SubjectId = subject.Id,
                Subject = subject,
                ActivityTagId = activityTagId,
                Duration = a.Duration,
                TotalDuration = a.TotalDuration,
                ActivityGroupId = a.ActivityGroupId,
                Active = a.Active,
                Comments = a.Comments,
            };
            await schedulesRepo.AddActivityAsync(activity, ct);
            if (a.FetId >= 0)
                fetIdToActivity[a.FetId] = activity;

            var teacherIdsAdded = new HashSet<int>();
            var teacherNames = SplitByPlus(a.TeacherName);
            foreach (var commentRef in a.CommentRefs)
            {
                var teacher = ResolveTeacher(
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
                if (!teacherByName.TryGetValue(teacherName, out var teacher))
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

            if (a.CommentRefs.Count > 0)
            {
                foreach (var commentRef in a.CommentRefs)
                {
                    var specializationExternalId = commentRef.SpecializationExternalId is > 0
                        ? commentRef.SpecializationExternalId
                        : null;
                    var studentsName = !string.IsNullOrWhiteSpace(commentRef.GroupExternalId)
                        ? $"group:{commentRef.GroupExternalId}"
                        : specializationExternalId.HasValue
                            ? $"specialization:{specializationExternalId.Value}"
                            : a.Students;

                    if (string.IsNullOrWhiteSpace(studentsName))
                        continue;

                    await schedulesRepo.AddActivityStudentsAsync(
                        new ActivityStudents
                        {
                            ActivityId = activity.Id,
                            StudentsName = studentsName,
                            PlanMatterProviderExternalId = commentRef.PlanMatterProviderExternalId,
                            FacultyExternalId = commentRef.FacultyExternalId,
                            MetaSpecializationExternalId = commentRef.MetaSpecializationExternalId,
                            StudyYearNumber = commentRef.StudyYearNumber,
                            GroupExternalId = commentRef.GroupExternalId,
                            SpecializationExternalId = specializationExternalId,
                            SubjectExternalId = commentRef.SubjectExternalId,
                            Activity = activity,
                        },
                        ct
                    );
                }
            }
            else
            {
                var studentNames = SplitByPlus(a.Students);
                foreach (var sn in studentNames)
                {
                    if (string.IsNullOrWhiteSpace(sn))
                        continue;
                    await schedulesRepo.AddActivityStudentsAsync(
                        new ActivityStudents
                        {
                            ActivityId = activity.Id,
                            StudentsName = sn,
                            Activity = activity,
                        },
                        ct
                    );
                }
            }
        }

        // ActivitySlots (timetable: activity at day/hour, including multi-hour activities)
        var hourOrder = data
            .Hours.Select((h, i) => (Name: h.Name, Index: i))
            .ToFrozenDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

        var activitySlots = new List<ActivitySlot>();
        foreach (var slot in data.ActivitySlots)
        {
            if (!fetIdToActivity.TryGetValue(slot.FetActivityId, out var activity))
                continue;
            if (!dayByName.TryGetValue(slot.DayName, out var day))
                continue;
            if (!hourOrder.TryGetValue(slot.HourName, out var startIndex))
                continue;

            var duration = fetActivityByFetId.TryGetValue(slot.FetActivityId, out var fa)
                ? fa.Duration
                : 1;

            for (var i = 0; i < duration && startIndex + i < data.Hours.Count; i++)
            {
                var hourName = data.Hours[startIndex + i].Name;
                if (!hourByName.TryGetValue(hourName, out var slotHour))
                    continue;
                activitySlots.Add(
                    new ActivitySlot
                    {
                        ScheduleId = scheduleId,
                        ActivityId = activity.Id,
                        DayId = day.Id,
                        HourId = slotHour.Id,
                    }
                );
            }
        }
        await schedulesRepo.AddActivitySlotsAsync(activitySlots, ct);

        return new FetImportResult(true, scheduleId, null);
    }

    private static IReadOnlyList<string> SplitByPlus(string value)
    {
        return value
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(v => !string.IsNullOrWhiteSpace(v))
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

            var subjectExternalId = commentRef.SubjectExternalId.Value;
            if (subjectByExternalId.TryGetValue(subjectExternalId, out var subjectById))
                return subjectById;

            if (!subjectByName.TryGetValue(subjectName, out var subjectByActivityName))
                continue;

            if (
                subjectByActivityName.ExternalSubjectId.HasValue
                && subjectByActivityName.ExternalSubjectId.Value != subjectExternalId
            )
                continue;

            subjectByActivityName.ExternalSubjectId = subjectExternalId;
            subjectByExternalId[subjectExternalId] = subjectByActivityName;
            return subjectByActivityName;
        }

        return subjectByName.TryGetValue(subjectName, out var subjectByFallbackName)
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

        if (teacherByExternalId.TryGetValue(teacherExternalId.Value, out var teacherById))
            return teacherById;

        foreach (var teacherName in teacherNames)
        {
            if (!teacherByName.TryGetValue(teacherName, out var teacherByActivityName))
                continue;

            if (
                teacherByActivityName.ExternalTeacherId.HasValue
                && teacherByActivityName.ExternalTeacherId.Value != teacherExternalId.Value
            )
                continue;

            teacherByActivityName.ExternalTeacherId = teacherExternalId.Value;
            teacherByExternalId[teacherExternalId.Value] = teacherByActivityName;
            return teacherByActivityName;
        }

        return null;
    }
}
