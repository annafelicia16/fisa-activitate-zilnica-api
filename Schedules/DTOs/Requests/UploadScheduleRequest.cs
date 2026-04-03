namespace FisaActivitateZilnicaApi.Schedules.DTOs.Requests;

public class UploadScheduleRequest
{
    public required IFormFile File { get; set; }
    public required string Name { get; set; }
    public required int Year { get; set; }
    public required int Semester { get; set; }
    public required bool OddWeek { get; set; }
}
