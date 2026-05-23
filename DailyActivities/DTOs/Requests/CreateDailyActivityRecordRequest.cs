using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;

public class CreateDailyActivityRecordRequest
{
    public required int ExternalTeacherId { get; set; }
    public required string DepartmentName { get; set; }
    public required string FacultyName { get; set; }
    public required string StudyProgram { get; set; }
    public required string CourseType { get; set; }
    public required int Year { get; set; }
    public required string GroupName { get; set; }
    public string? SubgroupName { get; set; }
    public required string SubjectName { get; set; }
    public required string RoomName { get; set; }
    public required RevenueType RevenueType { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.RegularHours;
    public required double ConventionalHours { get; set; }
    public string? Observations { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}
