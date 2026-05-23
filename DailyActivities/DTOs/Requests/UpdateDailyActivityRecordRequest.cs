using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;

public class UpdateDailyActivityRecordRequest
{
    public required string Id { get; set; }
    public string? DepartmentName { get; set; }
    public string? FacultyName { get; set; }
    public string? StudyProgram { get; set; }
    public string? CourseType { get; set; }
    public int? Year { get; set; }
    public string? GroupName { get; set; }
    public string? SubgroupName { get; set; }
    public string? SubjectName { get; set; }
    public string? RoomName { get; set; }
    public RevenueType? RevenueType { get; set; }
    public ActivityType? ActivityType { get; set; }
    public double? ConventionalHours { get; set; }
    public string? Observations { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ActivitySlotId { get; set; }
}
