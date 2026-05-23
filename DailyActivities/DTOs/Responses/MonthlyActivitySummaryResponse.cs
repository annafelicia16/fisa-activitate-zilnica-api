using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;

public sealed record MonthlyActivitySummaryResponse(
    int Year,
    int Month,
    int RecordCount,
    double TotalConventionalHours,
    MonthlySheetStatus Status
);
