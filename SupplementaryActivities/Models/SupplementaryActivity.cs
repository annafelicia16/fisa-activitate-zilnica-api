using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Models;

public class SupplementaryActivity
{
    [Key]
    [Column("Id")]
    public required string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("ExternalTeacherId")]
    public required int ExternalTeacherId { get; set; }

    [Required]
    [Column("Date")]
    public required DateTime Date { get; set; }

    [Required]
    [Column("ActivityType")]
    public required string ActivityType { get; set; }

    [Column("Observations")]
    public string? Observations { get; set; }

    [Required]
    [Column("TotalHours")]
    public required double TotalHours { get; set; }
}
