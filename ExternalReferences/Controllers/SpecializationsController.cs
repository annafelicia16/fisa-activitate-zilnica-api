using FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers;

public class SpecializationsController(ISpecializationsQueryService specializationsQueryService)
    : SpecializationsApiController
{
    private readonly ISpecializationsQueryService _specializationsQueryService =
        specializationsQueryService;

    public override async Task<ActionResult<IReadOnlyList<SpecializationResponse>>> GetSpecializations(
        string? faculty,
        string? search,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<SpecializationResponse> specializations =
            await _specializationsQueryService.SearchSpecializationsAsync(faculty, search, ct);

        return Ok(specializations);
    }
}
