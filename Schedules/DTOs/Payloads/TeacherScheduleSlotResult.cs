namespace FisaActivitateZilnicaApi.Schedules.DTOs.Payloads;

public sealed record TeacherScheduleSlotResult(
    int SlotId,
    int ScheduleId,
    int ActivityId,
    string HourName,
    string DayName,
    string SubjectName,
    string? CourseTypeTag,
    string? RoomName,
    int Duration,
    int? ExternalTeacherId,
    int? PlanMatterProviderExternalId,
    int? FacultyExternalId,
    int? MetaSpecializationExternalId,
    int? StudyYearNumber,
    int? Semester,
    int? PlataNB,
    string? OldActivityTag,
    string? GroupExternalId,
    string? GroupNames,
    int SpecializationExternalId,
    int? SubjectExternalId
);
