namespace FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;

public class UpdateSupplementaryActivityRequest
{
    public required string Id { get; set; }
    public DateTime? Date { get; set; }
    public string? ActivityType { get; set; }
    public string? Observations { get; set; }
    public double? TotalHours { get; set; }
}
