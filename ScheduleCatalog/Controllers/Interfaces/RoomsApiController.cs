using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class RoomsApiController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogRoomResponse>))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogRoomResponse>>> GetRooms(
        [FromQuery] string? faculty,
        [FromQuery] string? specialization,
        [FromQuery] int? year,
        [FromQuery] string? group,
        [FromQuery] string? subject,
        [FromQuery] string? search,
        CancellationToken ct = default
    );
}
