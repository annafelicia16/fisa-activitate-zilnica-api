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

    // Cycle is not stored on this row directly (ID_TipCiclu is unpopulated in AGSIS).
    // It is derived from the national study-program link below and, as a fallback,
    // the awarded diploma type. See StudyCycleResolver.

    // → N_PROGRAM_DE_STUDIU.ID (national RMU nomenclature). Null when unlinked.
    [Column("id_n_programdestudiu")]
    public long? IdProgramDeStudiu { get; set; }

    // → N_Tip_Diploma_Universitar.ID_Element (awarded diploma type).
    [Column("ID_N_Tip_Diploma_Universitar")]
    public int? IdTipDiplomaUniversitar { get; set; }
}
