using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;

public interface IDailyActivityRecordsRepository
{
    Task<GetDailyActivityRecordResponse?> GetDailyActivityRecordById(string id);
    Task<IEnumerable<GetDailyActivityRecordResponse>> QueryDailyActivityRecords(
        int externalTeacherId,
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
    Task<GetDailyActivityRecordResponse> CreateDailyActivityRecord(
        CreateDailyActivityRecordRequest request
    );
    Task<GetDailyActivityRecordResponse> UpdateDailyActivityRecord(
        UpdateDailyActivityRecordRequest request
    );
    Task<GetDailyActivityRecordResponse> DeleteDailyActivityRecord(string id);
}
