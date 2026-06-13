using FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers;

public class StudyYearsController(IStudyYearsQueryService studyYearsQueryService)
    : StudyYearsApiController
{
    private readonly IStudyYearsQueryService _studyYearsQueryService = studyYearsQueryService;

    public override async Task<ActionResult<IReadOnlyList<StudyYearResponse>>> GetStudyYears(
        string? faculty,
        string? specialization,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<StudyYearResponse> years = await _studyYearsQueryService.GetStudyYearsAsync(
            faculty,
            specialization,
            ct
        );

        return Ok(years);
    }
}
