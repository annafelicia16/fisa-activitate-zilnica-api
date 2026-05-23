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
    string? SpecializareName,
    // Friendly AGSIS dbo.Grupe.Nume (e.g. "8MF141"). Resolved from IdGrupa
    // when it points at a real group row; null otherwise (whole-class slots,
    // unparseable IDs, or the FET-only "GroupNames" text path).
    string? GrupaName
);
