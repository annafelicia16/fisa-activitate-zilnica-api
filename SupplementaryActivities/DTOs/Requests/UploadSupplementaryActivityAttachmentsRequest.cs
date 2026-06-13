namespace FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;

public class UploadSupplementaryActivityAttachmentsRequest
{
    public required List<IFormFile> Files { get; set; }
}
