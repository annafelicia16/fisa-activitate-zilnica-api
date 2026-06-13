namespace FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;

// Attachment metadata only — StoredFileName (the on-disk name) is deliberately
// not exposed; downloads go through the dedicated endpoint.
public class GetSupplementaryActivityAttachmentResponse
{
    public required string Id { get; set; }
    public required string SupplementaryActivityId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required long SizeBytes { get; set; }
    public required DateTime CreatedAt { get; set; }
}
