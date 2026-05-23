using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.Specializare
public class ExternalSpecialization
{
    [Key]
    [Column("ID_Specializare")]
    public long IdSpecializare { get; set; }

    [Required]
    [Column("Denumire")]
    public required string Denumire { get; set; }

    [Column("DenumireScurtaSpecializare")]
    public string? DenumireScurta { get; set; }

    [Column("DenumireEngleza")]
    public string? DenumireEngleza { get; set; }
}
