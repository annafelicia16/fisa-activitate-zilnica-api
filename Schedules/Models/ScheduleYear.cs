using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class ScheduleYear
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("Value")]
    public int Value { get; set; }

    public virtual ICollection<ScheduleSemester> ScheduleSemesters { get; set; } = [];
}
