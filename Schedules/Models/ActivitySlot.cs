using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.Schedules.Models;

/// <summary>
/// Represents a scheduled slot: an activity at a specific day and hour (and optionally room).
/// Used to build the full timetable for teacher activity sheets.
/// </summary>
public class ActivitySlot
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("ScheduleId")]
    public int ScheduleId { get; set; }

    [Required]
    [Column("ActivityId")]
    public int ActivityId { get; set; }

    [Required]
    [Column("DayId")]
    public int DayId { get; set; }

    [Required]
    [Column("HourId")]
    public int HourId { get; set; }

    [Column("RoomId")]
    public int? RoomId { get; set; }

    public virtual Schedule Schedule { get; set; } = null!;
    public virtual Activity Activity { get; set; } = null!;
    public virtual Day Day { get; set; } = null!;
    public virtual Hour Hour { get; set; } = null!;
    public virtual Room? Room { get; set; }
}
