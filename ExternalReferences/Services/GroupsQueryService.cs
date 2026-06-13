using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using FisaActivitateZilnicaApi.System;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services;

public class GroupsQueryService(IExternalReferencesRepository externalReferencesRepository)
    : IGroupsQueryService
{
    private readonly IExternalReferencesRepository _externalReferencesRepository =
        externalReferencesRepository;

    public async Task<IReadOnlyList<GroupResponse>> SearchGroupsAsync(
        string? faculty,
        string? specialization,
        int? year,
        string? search,
        CancellationToken ct = default
    )
    {
        string facultyName = (faculty ?? string.Empty).Trim();
        string specializationName = (specialization ?? string.Empty).Trim();
        if (facultyName.Length == 0 || specializationName.Length == 0 || year is not > 0)
            return [];

        var names = await _externalReferencesRepository.SearchGroupNamesAsync(
            facultyName,
            specializationName,
            year.Value,
            AcademicCalendar.CurrentYearLabel(DateTime.Now),
            search,
            ct
        );

        return names.Select(name => new GroupResponse(name)).ToList();
    }
}
