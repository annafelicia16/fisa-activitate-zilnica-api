using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;

namespace FisaActivitateZilnicaApi.DailyActivities.Services;

public class DailyActivityRecordsCommandService(
    IDailyActivityRecordsRepository dailyActivityRecordsRepository
) : IDailyActivityRecordsCommandService
{
    private readonly IDailyActivityRecordsRepository _dailyActivityRecordsRepository =
        dailyActivityRecordsRepository;

    public async Task<GetDailyActivityRecordResponse> CreateDailyActivityRecordAsync(
        CreateDailyActivityRecordRequest request
    )
    {
        ValidateCreateRequest(request);
        return await _dailyActivityRecordsRepository.CreateDailyActivityRecord(request);
    }

    public async Task<GetDailyActivityRecordResponse> UpdateDailyActivityRecordAsync(
        UpdateDailyActivityRecordRequest request
    )
    {
        ValidateUpdateRequest(request);
        await EnsureRecordExistsAsync(request.Id);

        return await _dailyActivityRecordsRepository.UpdateDailyActivityRecord(request);
    }

    public async Task<GetDailyActivityRecordResponse> DeleteDailyActivityRecordAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Route parameter 'id' is required.");

        await EnsureRecordExistsAsync(id);

        return await _dailyActivityRecordsRepository.DeleteDailyActivityRecord(id);
    }

    private async Task EnsureRecordExistsAsync(string id)
    {
        GetDailyActivityRecordResponse? existingRecord =
            await _dailyActivityRecordsRepository.GetDailyActivityRecordById(id);

        if (existingRecord == null)
            throw new KeyNotFoundException("Daily activity record does not exist.");
    }

    private static void ValidateCreateRequest(CreateDailyActivityRecordRequest request)
    {
        if (request.ExternalTeacherId <= 0)
            throw new ArgumentException("Field 'externalTeacherId' must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.DepartmentName))
            throw new ArgumentException("Field 'departmentName' is required.");

        if (request.Year <= 0)
            throw new ArgumentException("Field 'year' must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.GroupName))
            throw new ArgumentException("Field 'groupName' is required.");

        if (string.IsNullOrWhiteSpace(request.SubjectName))
            throw new ArgumentException("Field 'subjectName' is required.");

        if (string.IsNullOrWhiteSpace(request.RoomName))
            throw new ArgumentException("Field 'roomName' is required.");

        if (request.StartDate > request.EndDate)
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");
    }

    private static void ValidateUpdateRequest(UpdateDailyActivityRecordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ArgumentException("Field 'id' is required.");

        if (request.Year.HasValue && request.Year.Value <= 0)
            throw new ArgumentException("Field 'year' must be greater than 0.");

        if (
            request.StartDate.HasValue
            && request.EndDate.HasValue
            && request.StartDate > request.EndDate
        )
            throw new ArgumentException("'startDate' cannot be greater than 'endDate'.");
    }
}
