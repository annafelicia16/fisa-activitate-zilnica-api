using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using FisaActivitateZilnicaApi.System;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services;

public class StudyYearsQueryService(IExternalReferencesRepository externalReferencesRepository)
    : IStudyYearsQueryService
{
    private readonly IExternalReferencesRepository _externalReferencesRepository =
        externalReferencesRepository;

    public async Task<IReadOnlyList<StudyYearResponse>> GetStudyYearsAsync(
        string? faculty,
        string? specialization,
        CancellationToken ct = default
    )
    {
        string facultyName = (faculty ?? string.Empty).Trim();
        string specializationName = (specialization ?? string.Empty).Trim();
        if (facultyName.Length == 0 || specializationName.Length == 0)
            return [];

        var years = await _externalReferencesRepository.GetStudyYearsByFacultyAndSpecializationAsync(
            facultyName,
            specializationName,
            AcademicCalendar.CurrentYearLabel(DateTime.Now),
            ct
        );

        // AnStudiu rows are per-faculty entities; the same numeric year can show
        // up through several rows — collapse to one entry per number.
        return years
            .Where(y => y.NrAnStudiu is > 0)
            .GroupBy(y => y.NrAnStudiu!.Value)
            .OrderBy(g => g.Key)
            .Select(g => new StudyYearResponse(g.Key, g.First().Denumire))
            .ToList();
    }
}
