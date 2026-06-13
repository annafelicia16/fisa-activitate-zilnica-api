using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class GroupsApiController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<GroupResponse>))]
    public abstract Task<ActionResult<IReadOnlyList<GroupResponse>>> GetGroups(
        [FromQuery] string? faculty,
        [FromQuery] string? specialization,
        [FromQuery] int? year,
        [FromQuery] string? search,
        CancellationToken ct = default
    );
}
