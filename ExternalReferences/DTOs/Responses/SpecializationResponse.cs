namespace FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;

// Cycle is "Bachelor" / "Master" / "Doctorate", or null when AGSIS does not let us
// determine it. See StudyCycleResolver.
public sealed record SpecializationResponse(
    long IdSpecializare,
    string Name,
    string? ShortName,
    string? Cycle
);
