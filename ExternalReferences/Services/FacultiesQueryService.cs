using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using FisaActivitateZilnicaApi.System;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services;

public class FacultiesQueryService(IExternalReferencesRepository externalReferencesRepository)
    : IFacultiesQueryService
{
    private readonly IExternalReferencesRepository _externalReferencesRepository =
        externalReferencesRepository;

    public async Task<IReadOnlyList<FacultyResponse>> SearchFacultiesAsync(
        string? search,
        CancellationToken ct = default
    )
    {
        var faculties = await _externalReferencesRepository.SearchFacultiesAsync(
            search,
            AcademicCalendar.CurrentYearLabel(DateTime.Now),
            ct
        );

        return faculties
            .Select(f => new FacultyResponse((int)f.IdFacultate, f.Denumire, f.DenumireScurta))
            .ToList();
    }
}
