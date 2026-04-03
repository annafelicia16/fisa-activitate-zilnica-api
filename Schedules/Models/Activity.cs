using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class Activity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ScheduleId")]
    public int? ScheduleId { get; set; }

    [Required]
    [Column("SubjectId")]
    public int SubjectId { get; set; }

    [Column("ActivityTagId")]
    public int? ActivityTagId { get; set; }

    [Required]
    [Column("Duration")]
    public int Duration { get; set; }

    [Required]
    [Column("TotalDuration")]
    public int TotalDuration { get; set; }

    [Required]
    [Column("ActivityGroupId")]
    public int ActivityGroupId { get; set; }

    [Column("NumberOfStudents")]
    public int? NumberOfStudents { get; set; }

    [Required]
    [Column("Active")]
    public bool Active { get; set; } = true;

    [Column("Comments")]
    public string? Comments { get; set; }

    public virtual Schedule? Schedule { get; set; }
    public virtual required Subject Subject { get; set; }
    public virtual ActivityTag? ActivityTag { get; set; }
    public virtual ICollection<ActivityTeacher> Teachers { get; set; } = [];
    public virtual ICollection<ActivityStudents> StudentsSets { get; set; } = [];
}
