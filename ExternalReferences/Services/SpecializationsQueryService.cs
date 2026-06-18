using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Models;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using FisaActivitateZilnicaApi.System;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services;

public class SpecializationsQueryService(
    IExternalReferencesRepository externalReferencesRepository
) : ISpecializationsQueryService
{
    private readonly IExternalReferencesRepository _externalReferencesRepository =
        externalReferencesRepository;

    public async Task<IReadOnlyList<SpecializationResponse>> SearchSpecializationsAsync(
        string? faculty,
        string? search,
        CancellationToken ct = default
    )
    {
        string facultyName = (faculty ?? string.Empty).Trim();
        if (facultyName.Length == 0)
            return [];

        var specializations = await _externalReferencesRepository.SearchSpecializationsByFacultyAsync(
            facultyName,
            AcademicCalendar.CurrentYearLabel(DateTime.Now),
            search,
            ct
        );

        // AGSIS keeps duplicate rows for the same program name (distinct IDs,
        // e.g. two "ERASMUS" entries) — collapse them by name for the dropdown.
        var distinct = specializations
            .GroupBy(s => s.Denumire)
            .Select(g => g.First())
            .ToList();

        IReadOnlyDictionary<int, StudyCycle> cyclesBySpecId =
            await _externalReferencesRepository.GetSpecializationCyclesByIdsAsync(
                distinct.Select(s => (int)s.IdSpecializare).ToArray(),
                ct
            );

        return distinct
            .Select(s => new SpecializationResponse(
                s.IdSpecializare,
                s.Denumire,
                s.DenumireScurta,
                cyclesBySpecId.TryGetValue((int)s.IdSpecializare, out StudyCycle cycle)
                    ? StudyCycleResolver.ToLabel(cycle)
                    : null
            ))
            .ToList();
    }
}
