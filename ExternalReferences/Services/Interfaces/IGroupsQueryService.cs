using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;

public interface IGroupsQueryService
{
    Task<IReadOnlyList<GroupResponse>> SearchGroupsAsync(
        string? faculty,
        string? specialization,
        int? year,
        string? search,
        CancellationToken ct = default
    );
}
