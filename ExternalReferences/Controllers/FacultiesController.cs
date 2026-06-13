using FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers;

public class FacultiesController(IFacultiesQueryService facultiesQueryService)
    : FacultiesApiController
{
    private readonly IFacultiesQueryService _facultiesQueryService = facultiesQueryService;

    public override async Task<ActionResult<IReadOnlyList<FacultyResponse>>> GetFaculties(
        string? search,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<FacultyResponse> faculties =
            await _facultiesQueryService.SearchFacultiesAsync(search, ct);

        return Ok(faculties);
    }
}
