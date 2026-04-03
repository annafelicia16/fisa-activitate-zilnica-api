using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FisaActivitateZilnicaApi.Schedules.Models;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class ActivityTeacher
{
    [Key]
    [Column("ActivityId")]
    public required int ActivityId { get; set; }

    [Key]
    [Required]
    [Column("TeacherId")]
    public required int TeacherId { get; set; }

    public virtual required Activity Activity { get; set; } = null!;
    public virtual required Teacher Teacher { get; set; } = null!;
}
