namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;

// One row per calendar day in the requested range, with the calendar status the
// activity-sheet UI uses for its day dots. Status values are lower-case strings
// (free | future | missing | partial | completed) so the client can read them
// without an enum translation layer.
public sealed record DayStatusResponse(string Day, string Status);

public static class DayStatusValues
{
    public const string Free = "free";
    public const string Future = "future";
    public const string Missing = "missing";
    public const string Partial = "partial";
    public const string Completed = "completed";
}
