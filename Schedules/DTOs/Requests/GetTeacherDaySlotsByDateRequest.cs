namespace FisaActivitateZilnicaApi.Schedules.DTOs.Requests;

public class GetTeacherDaySlotsByDateRequest
{
    public int ExternalTeacherId { get; set; }
    public DateTime Date { get; set; }
}
