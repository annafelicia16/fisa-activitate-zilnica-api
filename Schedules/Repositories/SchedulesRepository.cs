using System.Linq.Expressions;
using FisaActivitateZilnicaApi.Data.Master;
using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Services.FetParser;
using Microsoft.EntityFrameworkCore;

namespace FisaActivitateZilnicaApi.Schedules.Repositories;

public class SchedulesRepository(MasterDbContext db) : ISchedulesRepository
{
    public Task<ScheduleYear?> GetScheduleYearByValueAsync(
        int value,
        CancellationToken ct = default
    ) => db.ScheduleYears.FirstOrDefaultAsync(scheduleYear => scheduleYear.Value == value, ct);

    public async Task AddScheduleYearAsync(
        ScheduleYear scheduleYear,
        CancellationToken ct = default
    )
    {
        db.ScheduleYears.Add(scheduleYear);
        await db.SaveChangesAsync(ct);
    }

    public Task<ScheduleSemester?> GetScheduleSemesterByYearAndNumberAsync(
        int scheduleYearId,
        int semesterNumber,
        CancellationToken ct = default
    ) =>
        db.ScheduleSemesters.FirstOrDefaultAsync(
            scheduleSemester =>
                scheduleSemester.ScheduleYearId == scheduleYearId
                && scheduleSemester.Number == semesterNumber,
            ct
        );

    public async Task AddScheduleSemesterAsync(
        ScheduleSemester scheduleSemester,
        CancellationToken ct = default
    )
    {
        db.ScheduleSemesters.Add(scheduleSemester);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> AnyScheduleAsync(
        Expression<Func<Schedule, bool>> predicate,
        CancellationToken ct = default
    ) => db.Schedules.AnyAsync(predicate, ct);

    public async Task AddScheduleAsync(Schedule schedule, CancellationToken ct = default)
    {
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddDaysAsync(IEnumerable<Day> days, CancellationToken ct = default)
    {
        db.Days.AddRange(days);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddHoursAsync(IEnumerable<Hour> hours, CancellationToken ct = default)
    {
        db.Hours.AddRange(hours);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddSubjectsAsync(IEnumerable<Subject> subjects, CancellationToken ct = default)
    {
        db.Subjects.AddRange(subjects);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddActivityTagsAsync(
        IEnumerable<ActivityTag> activityTags,
        CancellationToken ct = default
    )
    {
        db.ActivityTags.AddRange(activityTags);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddTeachersAsync(IEnumerable<Teacher> teachers, CancellationToken ct = default)
    {
        db.Teachers.AddRange(teachers);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddBuildingsAsync(
        IEnumerable<Building> buildings,
        CancellationToken ct = default
    )
    {
        db.Buildings.AddRange(buildings);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRoomsAsync(IEnumerable<Room> rooms, CancellationToken ct = default)
    {
        db.Rooms.AddRange(rooms);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddYearAsync(Year year, CancellationToken ct = default)
    {
        db.Years.Add(year);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddGroupAsync(Group group, CancellationToken ct = default)
    {
        db.Groups.Add(group);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddSubgroupAsync(Subgroup subgroup, CancellationToken ct = default)
    {
        db.Subgroups.Add(subgroup);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddActivityAsync(Activity activity, CancellationToken ct = default)
    {
        db.Activities.Add(activity);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddActivityTeacherAsync(
        ActivityTeacher activityTeacher,
        CancellationToken ct = default
    )
    {
        db.ActivityTeachers.Add(activityTeacher);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddActivityStudentsAsync(
        ActivityStudents activityStudents,
        CancellationToken ct = default
    )
    {
        db.ActivityStudents.Add(activityStudents);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddActivitySlotsAsync(
        IEnumerable<ActivitySlot> activitySlots,
        CancellationToken ct = default
    )
    {
        db.ActivitySlots.AddRange(activitySlots);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TeacherScheduleSlotResult>> GetTeacherDaySlotsByExternalIdsAsync(
        int scheduleId,
        string dayName,
        int externalTeacherId,
        int externalSubjectId,
        CancellationToken ct = default
    )
    {
        var results = await (
            from slot in db.ActivitySlots.AsNoTracking()
            join activity in db.Activities.AsNoTracking() on slot.ActivityId equals activity.Id
            join activityTeacher in db.ActivityTeachers.AsNoTracking() on activity.Id equals activityTeacher.ActivityId
            join teacher in db.Teachers.AsNoTracking() on activityTeacher.TeacherId equals teacher.Id
            join day in db.Days.AsNoTracking() on slot.DayId equals day.Id
            join hour in db.Hours.AsNoTracking() on slot.HourId equals hour.Id
            join subject in db.Subjects.AsNoTracking() on activity.SubjectId equals subject.Id
            join activityStudents in db.ActivityStudents.AsNoTracking() on activity.Id equals activityStudents.ActivityId
            join tag in db.ActivityTags.AsNoTracking() on activity.ActivityTagId equals tag.Id into tagJoin
            from tag in tagJoin.DefaultIfEmpty()
            join room in db.Rooms.AsNoTracking() on slot.RoomId equals room.Id into roomJoin
            from room in roomJoin.DefaultIfEmpty()
            where
                slot.ScheduleId == scheduleId
                && day.Name == dayName
                && teacher.ExternalTeacherId == externalTeacherId
                && activityStudents.SubjectExternalId == externalSubjectId
            orderby hour.Id, activity.Id, activityStudents.Id
            select new TeacherScheduleSlotResult(
                slot.Id,
                slot.ScheduleId,
                activity.Id,
                hour.Name,
                day.Name,
                subject.Name,
                tag != null ? tag.Name : null,
                room != null ? room.Name : null,
                activity.Duration,
                teacher.ExternalTeacherId,
                activityStudents.PlanMatterProviderExternalId,
                activityStudents.FacultyExternalId,
                activityStudents.MetaSpecializationExternalId,
                activityStudents.StudyYearNumber,
                activityStudents.Semester,
                activityStudents.PlataNB,
                activityStudents.OldActivityTag,
                activityStudents.GroupExternalId,
                activityStudents.GroupNames,
                activityStudents.SpecializationExternalId ?? -1,
                activityStudents.SubjectExternalId ?? subject.ExternalSubjectId
            )
        ).ToListAsync(ct);

        return results;
    }

    public Task<int?> FindInternalTeacherIdAsync(
        int scheduleId,
        int externalTeacherId,
        CancellationToken ct = default
    ) =>
        db.Teachers.AsNoTracking()
            .Where(t => t.ScheduleId == scheduleId && t.ExternalTeacherId == externalTeacherId)
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TeacherScheduleSlotResult>> GetTeacherDaySlotsByInternalIdAsync(
        int scheduleId,
        IReadOnlyCollection<string> dayNames,
        int teacherId,
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        // Pull the raw slot rows plus each activity's Comments JSON.
        // Then parse the JSON in memory — this is the source of truth for
        // teacher-facing fields (grupa, plataNB, oldactivitytag, etc.) and avoids
        // depending on the ActivityStudents columns being backfilled.
        var rawRows = await (
            from slot in db.ActivitySlots.AsNoTracking()
            join activity in db.Activities.AsNoTracking() on slot.ActivityId equals activity.Id
            join activityTeacher in db.ActivityTeachers.AsNoTracking() on activity.Id equals activityTeacher.ActivityId
            join day in db.Days.AsNoTracking() on slot.DayId equals day.Id
            join hour in db.Hours.AsNoTracking() on slot.HourId equals hour.Id
            join subject in db.Subjects.AsNoTracking() on activity.SubjectId equals subject.Id
            join tag in db.ActivityTags.AsNoTracking() on activity.ActivityTagId equals tag.Id into tagJoin
            from tag in tagJoin.DefaultIfEmpty()
            join room in db.Rooms.AsNoTracking() on slot.RoomId equals room.Id into roomJoin
            from room in roomJoin.DefaultIfEmpty()
            where
                slot.ScheduleId == scheduleId
                && dayNames.Contains(day.Name)
                && activityTeacher.TeacherId == teacherId
            orderby hour.Id, activity.Id
            select new
            {
                SlotId = slot.Id,
                slot.ScheduleId,
                ActivityId = activity.Id,
                HourName = hour.Name,
                DayName = day.Name,
                SubjectName = subject.Name,
                TagName = tag != null ? tag.Name : null,
                RoomName = room != null ? room.Name : null,
                ActivityDuration = activity.Duration,
                ActivityComments = activity.Comments,
                SubjectExternalId = subject.ExternalSubjectId,
            }
        ).ToListAsync(ct);

        var dedup = rawRows
            .GroupBy(r => r.SlotId)
            .Select(g => g.First())
            .ToList();

        var results = new List<TeacherScheduleSlotResult>(dedup.Count);
        foreach (var r in dedup)
        {
            IReadOnlyList<FetActivityCommentRef> refs =
                FetXmlParser.ParseCommentRefs(r.ActivityComments);

            FetActivityCommentRef? match =
                refs.FirstOrDefault(x =>
                    x.SubjectExternalId.HasValue
                    && r.SubjectExternalId.HasValue
                    && x.SubjectExternalId == r.SubjectExternalId
                ) ?? refs.FirstOrDefault();

            results.Add(new TeacherScheduleSlotResult(
                r.SlotId,
                r.ScheduleId,
                r.ActivityId,
                r.HourName,
                r.DayName,
                r.SubjectName,
                r.TagName,
                r.RoomName,
                r.ActivityDuration,
                externalTeacherId,
                match?.PlanMatterProviderExternalId,
                match?.FacultyExternalId,
                match?.MetaSpecializationExternalId,
                match?.StudyYearNumber,
                match?.Semester,
                match?.PlataNB,
                match?.OldActivityTag,
                match?.GroupExternalId,
                match?.GroupNames,
                match?.SpecializationExternalId ?? -1,
                match?.SubjectExternalId ?? r.SubjectExternalId
            ));
        }

        return results;
    }

    public async Task<IReadOnlyList<Schedule>> GetSchedulesByExternalTeacherIdAsync(
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        return await db
            .Schedules.AsNoTracking()
            .Include(schedule => schedule.ScheduleSemester)
            .ThenInclude(scheduleSemester => scheduleSemester.ScheduleYear)
            .Where(schedule =>
                schedule.Teachers.Any(teacher => teacher.ExternalTeacherId == externalTeacherId)
            )
            .OrderByDescending(schedule => schedule.ScheduleSemester.ScheduleYear.Value)
            .ThenByDescending(schedule => schedule.ScheduleSemester.Number)
            .ThenByDescending(schedule => schedule.OddWeek)
            .ThenBy(schedule => schedule.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Schedule>> GetAllSchedulesAsync(CancellationToken ct = default)
    {
        return await db
            .Schedules.AsNoTracking()
            .Include(schedule => schedule.ScheduleSemester)
            .ThenInclude(scheduleSemester => scheduleSemester.ScheduleYear)
            .OrderByDescending(schedule => schedule.ScheduleSemester.ScheduleYear.Value)
            .ThenByDescending(schedule => schedule.ScheduleSemester.Number)
            .ThenByDescending(schedule => schedule.OddWeek)
            .ThenBy(schedule => schedule.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<int>> GetTeacherFacultyExternalIdsAsync(
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        // A teacher's faculties = distinct FacultyExternalId across every ActivityStudents
        // row attached to an Activity they teach (in any schedule).
        return await (
            from teacher in db.Teachers.AsNoTracking()
            join activityTeacher in db.ActivityTeachers.AsNoTracking()
                on teacher.Id equals activityTeacher.TeacherId
            join activityStudents in db.ActivityStudents.AsNoTracking()
                on activityTeacher.ActivityId equals activityStudents.ActivityId
            where
                teacher.ExternalTeacherId == externalTeacherId
                && activityStudents.FacultyExternalId.HasValue
                && activityStudents.FacultyExternalId > 0
            select activityStudents.FacultyExternalId!.Value
        )
            .Distinct()
            .ToListAsync(ct);
    }

    public Task<ScheduleSemester?> GetActiveScheduleSemesterAsync(
        DateTime date,
        CancellationToken ct = default
    )
    {
        DateTime day = date.Date;
        return db
            .ScheduleSemesters.AsNoTracking()
            .Include(s => s.ScheduleYear)
            .Where(s => s.StartDate <= day && day <= s.EndDate)
            .OrderByDescending(s => s.ScheduleYear.Value)
            .ThenByDescending(s => s.Number)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<
        IReadOnlyDictionary<(int ScheduleId, string DayName), int>
    > GetTeacherSlotCountsByDayNameAsync(
        int externalTeacherId,
        IReadOnlyCollection<int> scheduleIds,
        CancellationToken ct = default
    )
    {
        if (scheduleIds.Count == 0)
            return new Dictionary<(int, string), int>();

        // Distinct on slot.Id because ActivityTeachers is many-to-many: a slot's
        // activity may have multiple teachers and we'd otherwise double-count.
        var rows = await (
            from slot in db.ActivitySlots.AsNoTracking()
            join activity in db.Activities.AsNoTracking() on slot.ActivityId equals activity.Id
            join activityTeacher in db.ActivityTeachers.AsNoTracking()
                on activity.Id equals activityTeacher.ActivityId
            join teacher in db.Teachers.AsNoTracking()
                on activityTeacher.TeacherId equals teacher.Id
            join day in db.Days.AsNoTracking() on slot.DayId equals day.Id
            where
                teacher.ExternalTeacherId == externalTeacherId
                && scheduleIds.Contains(slot.ScheduleId)
            select new
            {
                slot.ScheduleId,
                DayName = day.Name,
                SlotId = slot.Id,
            }
        )
            .Distinct()
            .ToListAsync(ct);

        return rows
            .GroupBy(r => (r.ScheduleId, r.DayName))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<int> BackfillActivityStudentsCommentRefsAsync(
        CancellationToken ct = default
    )
    {
        var activities = await db
            .Activities.Where(a => a.Comments != null && a.Comments != "")
            .Select(a => new { a.Id, a.Comments })
            .ToListAsync(ct);

        int updated = 0;
        foreach (var activity in activities)
        {
            IReadOnlyList<FetActivityCommentRef> refs = FetXmlParser.ParseCommentRefs(
                activity.Comments
            );
            if (refs.Count == 0)
                continue;

            List<ActivityStudents> studentRows = await db
                .ActivityStudents.Where(s => s.ActivityId == activity.Id)
                .ToListAsync(ct);

            foreach (var row in studentRows)
            {
                FetActivityCommentRef? matched = refs.FirstOrDefault(r =>
                    r.SubjectExternalId.HasValue
                    && row.SubjectExternalId.HasValue
                    && r.SubjectExternalId == row.SubjectExternalId
                ) ?? refs[0];

                row.Semester = matched.Semester ?? row.Semester;
                row.PlataNB = matched.PlataNB ?? row.PlataNB;
                row.OldActivityTag = matched.OldActivityTag ?? row.OldActivityTag;
                row.GroupNames = matched.GroupNames ?? row.GroupNames;

                if (
                    string.IsNullOrEmpty(row.GroupExternalId)
                    && !string.IsNullOrEmpty(matched.GroupExternalId)
                )
                {
                    row.GroupExternalId = matched.GroupExternalId;
                }

                updated++;
            }
        }

        if (updated > 0)
            await db.SaveChangesAsync(ct);

        return updated;
    }

    // Tracked (no AsNoTracking) so the caller can mutate the rows and persist via
    // SaveChangesAsync. Only rows with a faculty id participate in the catalog.
    public Task<List<ActivityStudents>> GetActivityStudentsForBackfillAsync(
        CancellationToken ct = default
    ) => db.ActivityStudents.Where(s => s.FacultyExternalId != null).ToListAsync(ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
