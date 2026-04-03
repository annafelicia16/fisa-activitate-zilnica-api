namespace FisaActivitateZilnicaApi.Schedules.DTOs.Requests;

public class GetTeacherScheduleSlotsRequest
{
    public int ScheduleId { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int ExternalTeacherId { get; set; }
    public int ExternalSubjectId { get; set; }
}
