namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;

// Summary of an auto-fill run. CreatedCount records were generated from missing
// scheduled slots; SkippedCount slots were left out because they lacked a
// required field (e.g. unresolved study program / group / room) and so couldn't
// form a valid record.
public sealed record AutoFillDailyActivityRecordsResponse(
    int CreatedCount,
    int SkippedCount,
    IReadOnlyList<GetDailyActivityRecordResponse> Created
);
