using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class FacultiesApiController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<FacultyResponse>))]
    public abstract Task<ActionResult<IReadOnlyList<FacultyResponse>>> GetFaculties(
        [FromQuery] string? search,
        CancellationToken ct = default
    );
}
