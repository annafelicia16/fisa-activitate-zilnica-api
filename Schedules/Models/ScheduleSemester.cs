using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class ScheduleSemester
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("ScheduleYearId")]
    public int ScheduleYearId { get; set; }

    [Required]
    [Column("Number")]
    public int Number { get; set; }

    [Required]
    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("EndDate")]
    public DateTime EndDate { get; set; }

    public virtual ScheduleYear ScheduleYear { get; set; } = null!;
    public virtual ICollection<Schedule> Schedules { get; set; } = [];
}
