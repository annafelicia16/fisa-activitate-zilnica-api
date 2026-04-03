namespace FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;

public sealed record FetImportResult(bool Success, int? ScheduleId, string? ErrorMessage);
