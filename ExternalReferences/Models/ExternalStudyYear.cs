using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.AnStudiu — study years per faculty (Denumire is the Roman numeral
// label "I".."IV", NrAnStudiu the numeric form). Groups reference it via
// Grupe.Id_AnStudiu.
public class ExternalStudyYear
{
    [Key]
    [Column("ID_AnStudiu")]
    public long IdAnStudiu { get; set; }

    [Required]
    [Column("Denumire")]
    public required string Denumire { get; set; }

    [Column("ID_Facultate")]
    public long? IdFacultate { get; set; }

    [Column("NrAnStudiu")]
    public int? NrAnStudiu { get; set; }
}
