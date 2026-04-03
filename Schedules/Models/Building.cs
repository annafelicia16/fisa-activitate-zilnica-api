using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FisaActivitateZilnicaApi.Schedules.Models;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class Building
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ScheduleId")]
    public int? ScheduleId { get; set; }

    [Required]
    [Column("Name")]
    public required string Name { get; set; } = string.Empty;

    [Column("Comments")]
    public string? Comments { get; set; }

    public virtual Schedule? Schedule { get; set; }
    public virtual ICollection<Room> Rooms { get; set; } = [];
}
