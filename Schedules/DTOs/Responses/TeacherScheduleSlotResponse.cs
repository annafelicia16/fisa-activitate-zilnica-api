namespace FisaActivitateZilnicaApi.Schedules.DTOs.Responses;

public sealed record TeacherScheduleSlotResponse(
    int SlotId,
    int ScheduleId,
    int ActivityId,
    string HourName,
    string DayName,
    string SubjectName,
    int? IdProfesor,
    int? IdPlanmateriePrestator,
    int? IdFacultate,
    int? IdMetaspecializare,
    int? NrAnStudii,
    string? IdGrupa,
    int IdSpecializare,
    int? IdMaterie
);
