namespace FisaActivitateZilnicaApi.Schedules.DTOs.Requests;

public class UploadScheduleRequest
{
    public required IFormFile OddWeekFile { get; set; }
    public required IFormFile EvenWeekFile { get; set; }
    public required string Name { get; set; }
    public required int Year { get; set; }
    public required int Semester { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}
