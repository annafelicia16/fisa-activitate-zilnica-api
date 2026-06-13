using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;

namespace FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;

public interface IStudyYearsQueryService
{
    Task<IReadOnlyList<StudyYearResponse>> GetStudyYearsAsync(
        string? faculty,
        string? specialization,
        CancellationToken ct = default
    );
}
