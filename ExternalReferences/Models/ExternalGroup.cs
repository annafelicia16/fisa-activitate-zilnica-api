using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.Grupe
public class ExternalGroup
{
    [Key]
    [Column("ID_Grupe")]
    public long IdGrupe { get; set; }

    [Column("ID_Specializare")]
    public long? IdSpecializare { get; set; }

    [Column("ID_Facultate")]
    public long? IdFacultate { get; set; }

    [Column("ID_Domeniu")]
    public long? IdDomeniu { get; set; }

    [Column("Id_AnStudiu")]
    public long? IdAnStudiu { get; set; }

    [Column("ID_AnUniv")]
    public long? IdAnUniv { get; set; }

    [Required]
    [Column("Nume")]
    public required string Nume { get; set; }
}
