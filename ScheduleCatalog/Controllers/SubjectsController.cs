using FisaActivitateZilnicaApi.ScheduleCatalog.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using FisaActivitateZilnicaApi.ScheduleCatalog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Controllers;

public class SubjectsController(IScheduleCatalogQueryService scheduleCatalogQueryService)
    : SubjectsApiController
{
    private readonly IScheduleCatalogQueryService _scheduleCatalogQueryService =
        scheduleCatalogQueryService;

    public override async Task<ActionResult<IReadOnlyList<CatalogSubjectResponse>>> GetSubjects(
        string? faculty,
        string? specialization,
        int? year,
        string? group,
        string? search,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<CatalogSubjectResponse> subjects =
            await _scheduleCatalogQueryService.SearchSubjectsAsync(
                faculty,
                specialization,
                year,
                group,
                search,
                ct
            );

        return Ok(subjects);
    }
}
