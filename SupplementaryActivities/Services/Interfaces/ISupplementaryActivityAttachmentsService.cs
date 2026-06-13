using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;

public interface ISupplementaryActivityAttachmentsService
{
    Task<IReadOnlyList<GetSupplementaryActivityAttachmentResponse>> UploadAttachmentsAsync(
        string activityId,
        IReadOnlyList<IFormFile> files,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<GetSupplementaryActivityAttachmentResponse>> GetAttachmentsForActivityAsync(
        string activityId
    );
    Task<(Stream Stream, string ContentType, string FileName)> OpenAttachmentForDownloadAsync(
        string attachmentId
    );
    Task DeleteAttachmentAsync(string attachmentId);

    // Disk-only cleanup used when a whole activity is deleted — the DB rows are
    // removed by the FK cascade.
    Task DeleteAllAttachmentFilesForActivityAsync(string activityId);
}
