using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.N_PROGRAM_DE_STUDIU — the national RMU/ANS study-program nomenclature.
// Versioned per academic year, so multiple rows share the same ID (the entity is
// keyless for that reason). Specializare.id_n_programdestudiu references ID; only
// the study cycle is needed here.
public class ExternalProgramOfStudy
{
    [Column("ID")]
    public int Id { get; set; }

    // 1 = Licență (bachelor), 2 = Master, 3 = Doctorat. Joins to N_CICLU_STUDII.
    [Column("ID_CICLU_STUDII")]
    public int? IdCicluStudii { get; set; }
}
