namespace FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;

public sealed record FetImportResult(
    bool Success,
    int? ScheduleYearId,
    int? ScheduleSemesterId,
    int? OddWeekScheduleId,
    int? EvenWeekScheduleId,
    string? ErrorMessage
);
