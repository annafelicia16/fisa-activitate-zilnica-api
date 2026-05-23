using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.Departament
public class ExternalDepartment
{
    [Key]
    [Column("ID_Departament")]
    public long IdDepartament { get; set; }

    [Required]
    [Column("DenumireDepartament")]
    public required string Denumire { get; set; }

    [Required]
    [Column("DenumireScurtaDepartament")]
    public required string DenumireScurta { get; set; }
}
