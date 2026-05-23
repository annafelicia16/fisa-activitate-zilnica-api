using System.Linq.Expressions;
using FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;
using FisaActivitateZilnicaApi.Schedules.Models;

namespace FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;

public interface ISchedulesRepository
{
    Task<ScheduleYear?> GetScheduleYearByValueAsync(int value, CancellationToken ct = default);
    Task AddScheduleYearAsync(ScheduleYear scheduleYear, CancellationToken ct = default);
    Task<ScheduleSemester?> GetScheduleSemesterByYearAndNumberAsync(
        int scheduleYearId,
        int semesterNumber,
        CancellationToken ct = default
    );
    Task AddScheduleSemesterAsync(
        ScheduleSemester scheduleSemester,
        CancellationToken ct = default
    );
    Task<bool> AnyScheduleAsync(
        Expression<Func<Schedule, bool>> predicate,
        CancellationToken ct = default
    );
    Task AddScheduleAsync(Schedule schedule, CancellationToken ct = default);
    Task AddDaysAsync(IEnumerable<Day> days, CancellationToken ct = default);
    Task AddHoursAsync(IEnumerable<Hour> hours, CancellationToken ct = default);
    Task AddSubjectsAsync(IEnumerable<Subject> subjects, CancellationToken ct = default);
    Task AddActivityTagsAsync(
        IEnumerable<ActivityTag> activityTags,
        CancellationToken ct = default
    );
    Task AddTeachersAsync(IEnumerable<Teacher> teachers, CancellationToken ct = default);
    Task AddBuildingsAsync(IEnumerable<Building> buildings, CancellationToken ct = default);
    Task AddRoomsAsync(IEnumerable<Room> rooms, CancellationToken ct = default);
    Task AddYearAsync(Year year, CancellationToken ct = default);
    Task AddGroupAsync(Group group, CancellationToken ct = default);
    Task AddSubgroupAsync(Subgroup subgroup, CancellationToken ct = default);
    Task AddActivityAsync(Activity activity, CancellationToken ct = default);
    Task AddActivityTeacherAsync(ActivityTeacher activityTeacher, CancellationToken ct = default);
    Task AddActivityStudentsAsync(
        ActivityStudents activityStudents,
        CancellationToken ct = default
    );
    Task AddActivitySlotsAsync(
        IEnumerable<ActivitySlot> activitySlots,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<TeacherScheduleSlotResult>> GetTeacherDaySlotsByExternalIdsAsync(
        int scheduleId,
        string dayName,
        int externalTeacherId,
        int externalSubjectId,
        CancellationToken ct = default
    );
    Task<int?> FindInternalTeacherIdAsync(
        int scheduleId,
        int externalTeacherId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<TeacherScheduleSlotResult>> GetTeacherDaySlotsByInternalIdAsync(
        int scheduleId,
        IReadOnlyCollection<string> dayNames,
        int teacherId,
        int externalTeacherId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<Schedule>> GetSchedulesByExternalTeacherIdAsync(
        int externalTeacherId,
        CancellationToken ct = default
    );
    Task<int> BackfillActivityStudentsCommentRefsAsync(CancellationToken ct = default);
}
