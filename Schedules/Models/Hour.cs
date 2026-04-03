using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class Hour
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ScheduleId")]
    public int? ScheduleId { get; set; }

    [Required]
    [Column("Name")]
    public required string Name { get; set; } = string.Empty;

    public virtual Schedule? Schedule { get; set; }
}
