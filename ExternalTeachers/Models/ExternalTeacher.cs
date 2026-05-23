using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Models;

public class ExternalTeacher
{
    [Key]
    [Column("ID_Profesor")]
    public required long IdProfesor { get; set; }

    [Required]
    [Column("Nume")]
    public required string Nume { get; set; }

    [Required]
    [Column("Prenume")]
    public required string Prenume { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("CNP")]
    public string? Cnp { get; set; }

    public override string ToString() =>
        $"{IdProfesor} - {Nume} {Prenume} - {Cnp ?? "N/A"} ({Email ?? "N/A"})";
}
