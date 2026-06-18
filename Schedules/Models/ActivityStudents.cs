using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FisaActivitateZilnicaApi.ExternalReferences.Models;

namespace FisaActivitateZilnicaApi.Schedules.Models;

public class ActivityStudents
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("ActivityId")]
    public required int ActivityId { get; set; }

    [Required]
    [Column("StudentsName")]
    public required string StudentsName { get; set; }

    [Column("PlanMatterProviderExternalId")]
    public int? PlanMatterProviderExternalId { get; set; }

    [Column("FacultyExternalId")]
    public int? FacultyExternalId { get; set; }

    [Column("MetaSpecializationExternalId")]
    public int? MetaSpecializationExternalId { get; set; }

    [Column("StudyYearNumber")]
    public int? StudyYearNumber { get; set; }

    [Column("Semester")]
    public int? Semester { get; set; }

    [Column("PlataNB")]
    public int? PlataNB { get; set; }

    [Column("OldActivityTag")]
    public string? OldActivityTag { get; set; }

    [Column("GroupExternalId")]
    public string? GroupExternalId { get; set; }

    [Column("GroupNames")]
    public string? GroupNames { get; set; }

    [Column("SpecializationExternalId")]
    public int? SpecializationExternalId { get; set; }

    [Column("SubjectExternalId")]
    public int? SubjectExternalId { get; set; }

    // AGSIS friendly names cached at import / backfill so the schedule-catalog
    // cascade reads them locally. EffectiveSpecializationExternalId is the
    // resolved specialization id (own > group's > shortname hint) used as the
    // stable cascade filter key when SpecializationExternalId is null.
    [Column("FacultyName")]
    public string? FacultyName { get; set; }

    [Column("SpecializationName")]
    public string? SpecializationName { get; set; }

    [Column("SubjectName")]
    public string? SubjectName { get; set; }

    [Column("ResolvedGroupName")]
    public string? ResolvedGroupName { get; set; }

    [Column("EffectiveSpecializationExternalId")]
    public int? EffectiveSpecializationExternalId { get; set; }

    // Study cycle of EffectiveSpecializationExternalId, resolved from AGSIS at import /
    // backfill. Null when it could not be determined. See StudyCycleResolver.
    [Column("SpecializationCycle")]
    public StudyCycle? SpecializationCycle { get; set; }

    public virtual required Activity Activity { get; set; }
}
