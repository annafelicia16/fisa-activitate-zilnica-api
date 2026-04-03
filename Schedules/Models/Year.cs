using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FisaActivitateZilnicaApi.Schedules.Models;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class Year
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("ScheduleId")]
    public int ScheduleId { get; set; }

    [Required]
    [Column("Name")]
    public required string Name { get; set; } = string.Empty;

    [Required]
    [Column("NumberOfStudents")]
    public int NumberOfStudents { get; set; }

    [Column("Comments")]
    public string? Comments { get; set; }

    public virtual Schedule? Schedule { get; set; }
    public virtual ICollection<Group> Groups { get; set; } = [];
}
