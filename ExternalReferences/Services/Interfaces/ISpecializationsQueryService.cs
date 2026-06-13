using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;

public interface ISpecializationsQueryService
{
    Task<IReadOnlyList<SpecializationResponse>> SearchSpecializationsAsync(
        string? faculty,
        string? search,
        CancellationToken ct = default
    );
}
