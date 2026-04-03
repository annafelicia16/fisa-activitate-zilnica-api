using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FisaActivitateZilnicaApi.Schedules.Models;
using SubjectModel = FisaActivitateZilnicaApi.Schedules.Models.Subject;
using YearModel = FisaActivitateZilnicaApi.Schedules.Models.Year;

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
    [Column("Year")]
    public int Year { get; set; }

    [Required]
    [Column("Semester")]
    public int Semester { get; set; }

    [Required]
    [Column("OddWeek")]
    public bool OddWeek { get; set; }

    public virtual ICollection<Teacher> Teachers { get; set; } = [];
    public virtual ICollection<SubjectModel> Subjects { get; set; } = [];
    public virtual ICollection<ActivityTag> ActivityTags { get; set; } = [];
    public virtual ICollection<Activity> Activities { get; set; } = [];
    public virtual ICollection<Day> Days { get; set; } = [];
    public virtual ICollection<Hour> Hours { get; set; } = [];
    public virtual ICollection<Building> Buildings { get; set; } = [];
    public virtual ICollection<Room> Rooms { get; set; } = [];
    public virtual ICollection<YearModel> Years { get; set; } = [];
}
