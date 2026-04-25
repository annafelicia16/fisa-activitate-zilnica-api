using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudyYearModel = FisaActivitateZilnicaApi.Schedules.Models.Year;
using SubjectModel = FisaActivitateZilnicaApi.Schedules.Models.Subject;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class Schedule
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("Name")]
    public required string Name { get; set; } = string.Empty;

    [Required]
    [Column("ScheduleSemesterId")]
    public int ScheduleSemesterId { get; set; }

    [Required]
    [Column("OddWeek")]
    public bool OddWeek { get; set; }

    public virtual ScheduleSemester ScheduleSemester { get; set; } = null!;
    public virtual ICollection<Teacher> Teachers { get; set; } = [];
    public virtual ICollection<SubjectModel> Subjects { get; set; } = [];
    public virtual ICollection<ActivityTag> ActivityTags { get; set; } = [];
    public virtual ICollection<Activity> Activities { get; set; } = [];
    public virtual ICollection<Day> Days { get; set; } = [];
    public virtual ICollection<Hour> Hours { get; set; } = [];
    public virtual ICollection<Building> Buildings { get; set; } = [];
    public virtual ICollection<Room> Rooms { get; set; } = [];
    public virtual ICollection<StudyYearModel> Years { get; set; } = [];
}
