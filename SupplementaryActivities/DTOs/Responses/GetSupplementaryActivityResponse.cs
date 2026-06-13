namespace FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;

public class GetSupplementaryActivityResponse
{
    public required string Id { get; set; }
    public required int ExternalTeacherId { get; set; }
    public required DateTime Date { get; set; }
    public required string ActivityType { get; set; }
    public string? Observations { get; set; }
    public required double TotalHours { get; set; }

    // Stitched in by the repository (not a model column) — drives the paperclip
    // indicator in the client's entries list.
    public int AttachmentCount { get; set; }
}
