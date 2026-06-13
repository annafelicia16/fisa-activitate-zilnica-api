using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;

public interface IFacultiesQueryService
{
    Task<IReadOnlyList<FacultyResponse>> SearchFacultiesAsync(
        string? search,
        CancellationToken ct = default
    );
}
