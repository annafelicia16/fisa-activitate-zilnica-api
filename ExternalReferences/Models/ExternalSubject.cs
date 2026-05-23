using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.Materie
public class ExternalSubject
{
    [Key]
    [Column("ID_Materie")]
    public long IdMaterie { get; set; }

    [Required]
    [Column("Denumire")]
    public required string Denumire { get; set; }

    [Column("DenumireScurta")]
    public string? DenumireScurta { get; set; }

    [Column("DenumireEngleza")]
    public string? DenumireEngleza { get; set; }

    [Column("ID_Facultate")]
    public long? IdFacultate { get; set; }
}
