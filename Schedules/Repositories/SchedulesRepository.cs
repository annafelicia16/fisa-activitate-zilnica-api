using System.Linq.Expressions;
using FisaActivitateZilnicaApi.Data.Master;
using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
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
                teacher.ExternalTeacherId,
                activityStudents.PlanMatterProviderExternalId,
                activityStudents.FacultyExternalId,
                activityStudents.MetaSpecializationExternalId,
                activityStudents.StudyYearNumber,
                activityStudents.GroupExternalId,
                activityStudents.SpecializationExternalId ?? -1,
                activityStudents.SubjectExternalId ?? subject.ExternalSubjectId
            )
        ).ToListAsync(ct);

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
}
