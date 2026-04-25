namespace FisaActivitateZilnicaApi.Schedules.DTOs.Responses;

public sealed record UploadScheduleResponse(
    int ScheduleYearId,
    int ScheduleSemesterId,
    int OddWeekScheduleId,
    int EvenWeekScheduleId
);
