using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.AnUniversitar. Denumire is a label like "An universitar 2025-2026";
// rows exist for future years too, so "current" is matched by label, not by date
// columns (which are unreliable/NULL on many rows).
public class ExternalAcademicYear
{
    [Key]
    [Column("ID_AnUniv")]
    public long IdAnUniv { get; set; }

    [Required]
    [Column("Denumire")]
    public required string Denumire { get; set; }
}
