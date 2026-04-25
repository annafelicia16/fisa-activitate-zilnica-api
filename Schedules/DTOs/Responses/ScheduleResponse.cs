namespace FisaActivitateZilnicaApi.Schedules.DTOs.Responses;

public sealed record ScheduleResponse(
    int Id,
    string Name,
    int Year,
    int Semester,
    DateTime StartDate,
    DateTime EndDate,
    bool OddWeek
);
