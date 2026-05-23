using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.DailyActivities.Models;

public class DailyActivityRecord
{
    [Key]
    [Column("Id")]
    public required string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("ExternalTeacherId")]
    public required int ExternalTeacherId { get; set; }

    [Required]
    [Column("DepartmentName")]
    public required string DepartmentName { get; set; }

    [Required]
    [Column("FacultyName")]
    public required string FacultyName { get; set; }

    [Required]
    [Column("StudyProgram")]
    public required string StudyProgram { get; set; }

    [Required]
    [Column("CourseType")]
    public required string CourseType { get; set; }

    [Required]
    [Column("Year")]
    public required int Year { get; set; }

    [Required]
    [Column("GroupName")]
    public required string GroupName { get; set; }

    [Required]
    [Column("SubjectName")]
    public required string SubjectName { get; set; }

    [Required]
    [Column("RoomName")]
    public required string RoomName { get; set; }

    [Required]
    [Column("RevenueType")]
    public required RevenueType RevenueType { get; set; }

    [Required]
    [Column("ActivityType")]
    public required ActivityType ActivityType { get; set; }

    [Required]
    [Column("ConventionalHours")]
    public required double ConventionalHours { get; set; }

    [Column("Observations")]
    public string? Observations { get; set; }

    [Required]
    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("EndDate")]
    public DateTime EndDate { get; set; }

    // FK to ActivitySlot. Nullable so ad-hoc activity (not tied to a scheduled
    // slot) can still be recorded. When set, identifies exactly which scheduled
    // slot this record satisfies — used to filter the scheduled-today list.
    [Column("ActivitySlotId")]
    public int? ActivitySlotId { get; set; }
}
