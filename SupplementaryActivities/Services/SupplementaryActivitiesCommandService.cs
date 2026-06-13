using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Services;

public class SupplementaryActivitiesCommandService(
    ISupplementaryActivitiesRepository supplementaryActivitiesRepository,
    ISupplementaryActivityAttachmentsService supplementaryActivityAttachmentsService
) : ISupplementaryActivitiesCommandService
{
    private readonly ISupplementaryActivitiesRepository _supplementaryActivitiesRepository =
        supplementaryActivitiesRepository;
    private readonly ISupplementaryActivityAttachmentsService _supplementaryActivityAttachmentsService =
        supplementaryActivityAttachmentsService;

    public async Task<GetSupplementaryActivityResponse> CreateSupplementaryActivityAsync(
        CreateSupplementaryActivityRequest request
    )
    {
        ValidateCreateRequest(request);
        return await _supplementaryActivitiesRepository.CreateSupplementaryActivity(request);
    }

    public async Task<GetSupplementaryActivityResponse> UpdateSupplementaryActivityAsync(
        UpdateSupplementaryActivityRequest request
    )
    {
        ValidateUpdateRequest(request);
        await EnsureExistsAsync(request.Id);
        return await _supplementaryActivitiesRepository.UpdateSupplementaryActivity(request);
    }

    public async Task<GetSupplementaryActivityResponse> DeleteSupplementaryActivityAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Route parameter 'id' is required.");

        await EnsureExistsAsync(id);

        // Attachment rows die with the activity via the FK cascade; the disk
        // files need explicit cleanup, after the DB delete succeeds.
        GetSupplementaryActivityResponse deleted =
            await _supplementaryActivitiesRepository.DeleteSupplementaryActivity(id);
        await _supplementaryActivityAttachmentsService.DeleteAllAttachmentFilesForActivityAsync(
            id
        );
        return deleted;
    }

    private async Task EnsureExistsAsync(string id)
    {
        GetSupplementaryActivityResponse? existing =
            await _supplementaryActivitiesRepository.GetSupplementaryActivityById(id);

        if (existing == null)
            throw new KeyNotFoundException("Supplementary activity does not exist.");
    }

    private static void ValidateCreateRequest(CreateSupplementaryActivityRequest request)
    {
        if (request.ExternalTeacherId <= 0)
            throw new ArgumentException("Field 'externalTeacherId' must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.ActivityType))
            throw new ArgumentException("Field 'activityType' is required.");

        if (request.TotalHours < 0)
            throw new ArgumentException("Field 'totalHours' must be 0 or greater.");
    }

    private static void ValidateUpdateRequest(UpdateSupplementaryActivityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ArgumentException("Field 'id' is required.");

        if (request.TotalHours.HasValue && request.TotalHours.Value < 0)
            throw new ArgumentException("Field 'totalHours' must be 0 or greater.");
    }
}
