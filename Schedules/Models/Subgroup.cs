using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class Subgroup
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("Name")]
    public required string Name { get; set; } = string.Empty;

    [Required]
    [Column("GroupId")]
    public int GroupId { get; set; }

    [Required]
    [Column("NumberOfStudents")]
    public int NumberOfStudents { get; set; }

    [Column("Comments")]
    public string? Comments { get; set; }

    public virtual required Group Group { get; set; }
}
