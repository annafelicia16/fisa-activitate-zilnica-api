namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;

// Auto-fills every missing DailyActivityRecord in [StartDate, EndDate] from the
// teacher's schedule. A day's "missing" slots are the scheduled slots that don't
// yet have a record pointing at them (matched by ActivitySlotId) — the same
// definition the per-day "Programat" list uses.
public class AutoFillDailyActivityRecordsRequest
{
    public required int ExternalTeacherId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    // Optional department label stamped on the generated records. Falls back to
    // each slot's faculty name when omitted, mirroring the manual form's
    // `dept || faculty` rule.
    public string? DepartmentName { get; set; }
}
