namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;

public class UploadDailyActivityRecordAttachmentsRequest
{
    public required List<IFormFile> Files { get; set; }
}
