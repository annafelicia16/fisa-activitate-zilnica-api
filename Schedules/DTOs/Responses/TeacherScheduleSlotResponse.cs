namespace FisaActivitateZilnicaApi.Schedules.DTOs.Responses;

public sealed record TeacherScheduleSlotResponse(
    int SlotId,
    int ScheduleId,
    int ActivityId,
    string HourName,
    string DayName,
    string SubjectName,
    string? CourseTypeTag,
    string? RoomName,
    int Duration,
    int? IdProfesor,
    int? IdPlanmateriePrestator,
    int? IdFacultate,
    int? IdMetaspecializare,
    int? NrAnStudii,
    int? NrSemestruDinAn,
    int? PlataNB,
    string? OldActivityTag,
    string? IdGrupa,
    string? Grupa,
    int IdSpecializare,
    int? IdMaterie,
    string? MaterieName,
    string? MaterieShortName,
    string? FacultateName,
    string? SpecializareName
);
