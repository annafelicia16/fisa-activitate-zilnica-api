using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class SubjectsApiController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogSubjectResponse>))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogSubjectResponse>>> GetSubjects(
        [FromQuery] string? faculty,
        [FromQuery] string? specialization,
        [FromQuery] int? year,
        [FromQuery] string? group,
        [FromQuery] string? search,
        CancellationToken ct = default
    );
}
