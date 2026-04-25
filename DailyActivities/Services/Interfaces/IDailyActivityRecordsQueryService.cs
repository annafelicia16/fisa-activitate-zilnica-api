using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;

public interface IDailyActivityRecordsQueryService
{
    Task<GetDailyActivityRecordResponse> GetDailyActivityRecordByIdAsync(string id);
    Task<IEnumerable<GetDailyActivityRecordResponse>> QueryDailyActivityRecordsAsync(
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
    );
}
