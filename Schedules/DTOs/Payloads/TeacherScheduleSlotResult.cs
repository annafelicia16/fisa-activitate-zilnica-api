namespace FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;

public sealed record TeacherScheduleSlotResult(
    int SlotId,
    int ScheduleId,
    int ActivityId,
    string HourName,
    string DayName,
    string SubjectName,
    int? ExternalTeacherId,
    int? PlanMatterProviderExternalId,
    int? FacultyExternalId,
    int? MetaSpecializationExternalId,
    int? StudyYearNumber,
    string? GroupExternalId,
    int SpecializationExternalId,
    int? SubjectExternalId
);
