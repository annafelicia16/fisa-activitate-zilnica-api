using FisaActivitateZilnicaApi.DailyActivities.Models;

namespace FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;

public class GetDailyActivityRecordResponse
{
    public required string Id { get; set; }
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
    public required ActivityType ActivityType { get; set; }
    public required double ConventionalHours { get; set; }
    public string? Observations { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}
