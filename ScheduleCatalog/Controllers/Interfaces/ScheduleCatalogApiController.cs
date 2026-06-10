using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ScheduleCatalogApiController : ControllerBase
{
    [HttpGet("faculties")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogItemResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetFaculties(
        [FromQuery] int externalTeacherId,
        CancellationToken ct = default
    );

    [HttpGet("specializations")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogItemResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetSpecializations(
        [FromQuery] int externalTeacherId,
        [FromQuery] int facultyId,
        CancellationToken ct = default
    );

    [HttpGet("years")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogYearResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogYearResponse>>> GetYears(
        [FromQuery] int externalTeacherId,
        [FromQuery] int facultyId,
        [FromQuery] int specializationId,
        CancellationToken ct = default
    );

    [HttpGet("groups")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogGroupResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogGroupResponse>>> GetGroups(
        [FromQuery] int externalTeacherId,
        [FromQuery] int facultyId,
        [FromQuery] int specializationId,
        [FromQuery] int year,
        CancellationToken ct = default
    );

    [HttpGet("subjects")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogItemResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetSubjects(
        [FromQuery] int externalTeacherId,
        [FromQuery] int facultyId,
        [FromQuery] int specializationId,
        [FromQuery] int year,
        [FromQuery] string groupKey,
        CancellationToken ct = default
    );

    [HttpGet("rooms")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<CatalogRoomResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<CatalogRoomResponse>>> GetRooms(
        [FromQuery] int externalTeacherId,
        [FromQuery] int facultyId,
        [FromQuery] int specializationId,
        [FromQuery] int year,
        [FromQuery] string groupKey,
        [FromQuery] int subjectId,
        CancellationToken ct = default
    );
}
