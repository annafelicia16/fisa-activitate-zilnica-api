using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class Group
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("Name")]
    public required string Name { get; set; } = string.Empty;

    [Required]
    [Column("YearId")]
    public int YearId { get; set; }

    [Required]
    [Column("NumberOfStudents")]
    public int NumberOfStudents { get; set; }

    [Column("Comments")]
    public string? Comments { get; set; }

    public virtual required Year Year { get; set; }
    public virtual ICollection<Subgroup> Subgroups { get; set; } = [];
}
