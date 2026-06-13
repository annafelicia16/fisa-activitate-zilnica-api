using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;

namespace FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;

public interface IDailyActivityRecordAttachmentsService
{
    Task<IReadOnlyList<GetDailyActivityRecordAttachmentResponse>> UploadAttachmentsAsync(
        string recordId,
        IReadOnlyList<IFormFile> files,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<GetDailyActivityRecordAttachmentResponse>> GetAttachmentsForRecordAsync(
        string recordId
    );
    Task<(Stream Stream, string ContentType, string FileName)> OpenAttachmentForDownloadAsync(
        string attachmentId
    );
    Task DeleteAttachmentAsync(string attachmentId);

    // Disk-only cleanup used when a whole record is deleted — the DB rows are
    // removed by the FK cascade.
    Task DeleteAllAttachmentFilesForRecordAsync(string recordId);
}
