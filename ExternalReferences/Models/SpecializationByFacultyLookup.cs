namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// Projection used when resolving a specialization from its short name (e.g. "IE")
// scoped to a specific faculty via dbo.Grupe.
public sealed record SpecializationByFacultyLookup(
    long IdSpecializare,
    string Denumire,
    string? DenumireScurta,
    long FacultyId
);
