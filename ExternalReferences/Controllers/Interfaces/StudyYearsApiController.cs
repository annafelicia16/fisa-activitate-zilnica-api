using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class StudyYearsApiController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<StudyYearResponse>))]
    public abstract Task<ActionResult<IReadOnlyList<StudyYearResponse>>> GetStudyYears(
        [FromQuery] string? faculty,
        [FromQuery] string? specialization,
        CancellationToken ct = default
    );
}
