using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Controllers.Interfaces;

[ApiController]
[Route("api/[controller]")]
public abstract class SupplementaryActivitiesApiController : ControllerBase
{
    [HttpGet("get-by-id/{id}")]
    [ProducesResponseType(statusCode: 200, type: typeof(GetSupplementaryActivityResponse))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<
        ActionResult<GetSupplementaryActivityResponse>
    > GetSupplementaryActivityById([FromRoute] string id);

    [HttpGet("query")]
    [ProducesResponseType(
        statusCode: 200,
        type: typeof(IEnumerable<GetSupplementaryActivityResponse>)
    )]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<
        ActionResult<IEnumerable<GetSupplementaryActivityResponse>>
    > QuerySupplementaryActivities(
        [FromQuery] int teacherId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate
    );

    [HttpPost("create")]
    [ProducesResponseType(statusCode: 201, type: typeof(GetSupplementaryActivityResponse))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<
        ActionResult<GetSupplementaryActivityResponse>
    > CreateSupplementaryActivity([FromBody] CreateSupplementaryActivityRequest request);

    [HttpPut("update")]
    [ProducesResponseType(statusCode: 202, type: typeof(GetSupplementaryActivityResponse))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<
        ActionResult<GetSupplementaryActivityResponse>
    > UpdateSupplementaryActivity([FromBody] UpdateSupplementaryActivityRequest request);

    [HttpDelete("delete/{id}")]
    [ProducesResponseType(statusCode: 204, type: typeof(GetSupplementaryActivityResponse))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<
        ActionResult<GetSupplementaryActivityResponse>
    > DeleteSupplementaryActivity([FromRoute] string id);
}
