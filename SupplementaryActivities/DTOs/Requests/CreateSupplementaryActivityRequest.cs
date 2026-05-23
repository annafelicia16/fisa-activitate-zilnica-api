namespace FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;

public class CreateSupplementaryActivityRequest
{
    public required int ExternalTeacherId { get; set; }
    public required DateTime Date { get; set; }
    public required string ActivityType { get; set; }
    public string? Observations { get; set; }
    public required double TotalHours { get; set; }
}
