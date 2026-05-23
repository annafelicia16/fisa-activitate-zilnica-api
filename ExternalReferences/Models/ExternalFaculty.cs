using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.Facultate
public class ExternalFaculty
{
    [Key]
    [Column("ID_Facultate")]
    public long IdFacultate { get; set; }

    [Required]
    [Column("Denumire")]
    public required string Denumire { get; set; }

    [Column("DenumireScurta")]
    public string? DenumireScurta { get; set; }

    [Column("DenumireEngleza")]
    public string? DenumireEngleza { get; set; }
}
