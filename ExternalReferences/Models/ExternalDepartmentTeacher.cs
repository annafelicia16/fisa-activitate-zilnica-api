using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// AGSIS dbo.DepartamentProfesor — teacher↔department assignment, with an active flag
// and optional end date (DataPanaCand). One teacher can have multiple historical rows;
// the "current" department is the one with Activ=1 and DataPanaCand IS NULL.
public class ExternalDepartmentTeacher
{
    [Key]
    [Column("ID_DepartamentProfesor")]
    public long IdDepartamentProfesor { get; set; }

    [Required]
    [Column("ID_Departament")]
    public long IdDepartament { get; set; }

    [Required]
    [Column("ID_Profesor")]
    public long IdProfesor { get; set; }

    [Column("DataDeCand")]
    public DateTime DataDeCand { get; set; }

    [Column("DataPanaCand")]
    public DateTime? DataPanaCand { get; set; }

    [Column("DepartamentProfesorActiv")]
    public bool? Activ { get; set; }
}
