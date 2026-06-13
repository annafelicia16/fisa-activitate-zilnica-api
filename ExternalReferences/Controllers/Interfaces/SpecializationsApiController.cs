using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class SpecializationsApiController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<SpecializationResponse>))]
    public abstract Task<ActionResult<IReadOnlyList<SpecializationResponse>>> GetSpecializations(
        [FromQuery] string? faculty,
        [FromQuery] string? search,
        CancellationToken ct = default
    );
}
