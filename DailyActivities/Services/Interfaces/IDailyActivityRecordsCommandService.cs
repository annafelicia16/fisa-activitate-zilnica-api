using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;

namespace FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;

public interface IDailyActivityRecordsCommandService
{
    Task<GetDailyActivityRecordResponse> CreateDailyActivityRecordAsync(
        CreateDailyActivityRecordRequest request
    );
    Task<GetDailyActivityRecordResponse> UpdateDailyActivityRecordAsync(
        UpdateDailyActivityRecordRequest request
    );
    Task<GetDailyActivityRecordResponse> DeleteDailyActivityRecordAsync(string id);
}
