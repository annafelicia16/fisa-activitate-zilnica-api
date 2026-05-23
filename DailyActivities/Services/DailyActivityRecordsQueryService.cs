using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;

namespace FisaActivitateZilnicaApi.DailyActivities.Services;

public class DailyActivityRecordsQueryService(
    IDailyActivityRecordsRepository dailyActivityRecordsRepository
) : IDailyActivityRecordsQueryService
{
    private readonly IDailyActivityRecordsRepository _dailyActivityRecordsRepository =
        dailyActivityRecordsRepository;

    public async Task<GetDailyActivityRecordResponse> GetDailyActivityRecordByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Query parameter 'id' is required.");

        GetDailyActivityRecordResponse? dailyActivityRecord =
            await _dailyActivityRecordsRepository.GetDailyActivityRecordById(id);

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

        return await _dailyActivityRecordsRepository.QueryDailyActivityRecords(
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

        return await _dailyActivityRecordsRepository.GetMonthlySummaries(teacherId);
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
