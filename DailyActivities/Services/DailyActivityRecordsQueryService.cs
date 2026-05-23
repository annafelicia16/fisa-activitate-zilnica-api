using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Models;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;

namespace FisaActivitateZilnicaApi.DailyActivities.Services;

public class DailyActivityRecordsQueryService(
    IDailyActivityRecordsRepository dailyActivityRecordsRepository,
    ISchedulesRepository schedulesRepository
) : IDailyActivityRecordsQueryService
{
    private const int MaxDayStatusRangeDays = 366;

    private static readonly IReadOnlyDictionary<DayOfWeek, string[]> DayNameAliases =
        new Dictionary<DayOfWeek, string[]>
        {
            [DayOfWeek.Monday] = ["Luni", "Monday"],
            [DayOfWeek.Tuesday] = ["Marți", "Marti", "Tuesday"],
            [DayOfWeek.Wednesday] = ["Miercuri", "Wednesday"],
            [DayOfWeek.Thursday] = ["Joi", "Thursday"],
            [DayOfWeek.Friday] = ["Vineri", "Friday"],
            [DayOfWeek.Saturday] = ["Sâmbătă", "Sambata", "Saturday"],
            [DayOfWeek.Sunday] = ["Duminică", "Duminica", "Sunday"],
        };

    public async Task<GetDailyActivityRecordResponse> GetDailyActivityRecordByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Query parameter 'id' is required.");

        GetDailyActivityRecordResponse? dailyActivityRecord =
            await dailyActivityRecordsRepository.GetDailyActivityRecordById(id);

        if (dailyActivityRecord == null)
            throw new KeyNotFoundException("Daily activity record does not exist.");

        return dailyActivityRecord;
    }

    public async Task<IEnumerable<GetDailyActivityRecordResponse>> QueryDailyActivityRecordsAsync(
        int teacherId,
        string? departmentName,
        int? year,
        string? groupName,
        string? subgroupName,
        string? subjectName,
        string? roomName,
        RevenueType? revenueType,
        ActivityType? activityType,
        DateTime? startDate,
        DateTime? endDate
    )
    {
        ValidateQueryRequest(teacherId, year, startDate, endDate);

        return await dailyActivityRecordsRepository.QueryDailyActivityRecords(
            teacherId,
            departmentName,
            year,
            groupName,
            subgroupName,
            subjectName,
            roomName,
            revenueType,
            activityType,
            startDate,
            endDate
        );
    }

    public async Task<IEnumerable<MonthlyActivitySummaryResponse>> GetMonthlySummariesAsync(
        int teacherId
    )
    {
        if (teacherId <= 0)
            throw new ArgumentException("Query parameter 'teacherId' must be greater than 0.");

        return await dailyActivityRecordsRepository.GetMonthlySummaries(teacherId);
    }

    public async Task<IReadOnlyList<DayStatusResponse>> GetDailyStatusesAsync(
        int teacherId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default
    )
    {
        if (teacherId <= 0)
            throw new ArgumentException("Query parameter 'teacherId' must be greater than 0.");
        if (startDate > endDate)
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");
        if ((endDate.Date - startDate.Date).TotalDays > MaxDayStatusRangeDays)
            throw new ArgumentException(
                $"Date range cannot exceed {MaxDayStatusRangeDays} days."
            );

        DateTime today = DateTime.UtcNow.Date;
        DateTime rangeStart = startDate.Date;
        DateTime rangeEnd = endDate.Date;

        // 1. Teacher schedules (typically 2–4 rows per academic year).
        IReadOnlyList<Schedule> schedules =
            await schedulesRepository.GetSchedulesByExternalTeacherIdAsync(teacherId, ct);
        int[] scheduleIds = schedules.Select(s => s.Id).ToArray();

        // 2. Slot counts batched by (scheduleId, dayName) — one DB hit for the whole
        // semester. Slot count for a date is then a dictionary lookup.
        IReadOnlyDictionary<(int ScheduleId, string DayName), int> slotCounts =
            await schedulesRepository.GetTeacherSlotCountsByDayNameAsync(
                teacherId,
                scheduleIds,
                ct
            );

        // 3. Records in range — pass endDate + 1 day so the underlying < endDate filter
        // includes the entire endDate.
        IEnumerable<GetDailyActivityRecordResponse> records =
            await dailyActivityRecordsRepository.QueryDailyActivityRecords(
                teacherId,
                departmentName: null,
                year: null,
                groupName: null,
                subgroupName: null,
                subjectName: null,
                roomName: null,
                revenueType: null,
                activityType: null,
                startDate: rangeStart,
                endDate: rangeEnd.AddDays(1)
            );

        Dictionary<DateTime, int> recordsByDate = records
            .GroupBy(r => r.StartDate.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var results = new List<DayStatusResponse>();
        for (DateTime date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            int scheduled = ScheduledSlotsFor(date, schedules, slotCounts);
            int recorded = recordsByDate.GetValueOrDefault(date, 0);
            results.Add(
                new DayStatusResponse(
                    date.ToString("yyyy-MM-dd"),
                    ResolveStatus(date, today, scheduled, recorded)
                )
            );
        }

        return results;
    }

    private static string ResolveStatus(
        DateTime date,
        DateTime today,
        int scheduled,
        int recorded
    )
    {
        if (scheduled == 0)
            return DayStatusValues.Free;
        if (recorded == 0)
            return date > today ? DayStatusValues.Future : DayStatusValues.Missing;
        if (recorded < scheduled)
            return DayStatusValues.Partial;
        return DayStatusValues.Completed;
    }

    // Mirrors the schedule-picking heuristic in SchedulesQueryService.ResolveScheduleForDate:
    // pick the schedule whose semester contains the date and whose OddWeek flag matches
    // the parity of the week-from-semester-start (week 0 = odd by convention).
    private static int ScheduledSlotsFor(
        DateTime date,
        IReadOnlyList<Schedule> schedules,
        IReadOnlyDictionary<(int ScheduleId, string DayName), int> slotCounts
    )
    {
        Schedule? matching = ResolveScheduleForDate(schedules, date);
        if (matching is null)
            return 0;

        if (!DayNameAliases.TryGetValue(date.DayOfWeek, out string[]? aliases))
            return 0;

        int total = 0;
        foreach (string alias in aliases)
        {
            if (slotCounts.TryGetValue((matching.Id, alias), out int count))
                total += count;
        }
        return total;
    }

    private static Schedule? ResolveScheduleForDate(
        IReadOnlyList<Schedule> schedules,
        DateTime date
    )
    {
        Schedule? candidate = schedules.FirstOrDefault(s =>
            s.ScheduleSemester != null
            && s.ScheduleSemester.StartDate.Date <= date
            && date <= s.ScheduleSemester.EndDate.Date
        );
        if (candidate is null)
            return null;

        ScheduleSemester semester = candidate.ScheduleSemester;
        int daysFromStart = (date - semester.StartDate.Date).Days;
        int weeksFromStart = daysFromStart / 7;
        bool isOddWeek = weeksFromStart % 2 == 0;

        return schedules.FirstOrDefault(s =>
            s.ScheduleSemesterId == semester.Id && s.OddWeek == isOddWeek
        ) ?? candidate;
    }

    private static void ValidateQueryRequest(
        int teacherId,
        int? year,
        DateTime? startDate,
        DateTime? endDate
    )
    {
        if (teacherId <= 0)
            throw new ArgumentException("Query parameter 'teacherId' must be greater than 0.");

        if (year.HasValue && year.Value <= 0)
            throw new ArgumentException("Query parameter 'year' must be greater than 0.");

        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");
    }
}
