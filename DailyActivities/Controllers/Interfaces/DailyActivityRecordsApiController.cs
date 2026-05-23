using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.DailyActivities.Controllers.Interfaces;

[ApiController]
[Route("api/[controller]")]
public abstract class DailyActivityRecordsApiController : ControllerBase
{
    [HttpGet("get-by-id/{id}")]
    [ProducesResponseType(statusCode: 200, type: typeof(GetDailyActivityRecordResponse))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<ActionResult<GetDailyActivityRecordResponse>> GetDailyActivityRecordById(
        [FromRoute] string id
    );

    [HttpGet("query")]
    [ProducesResponseType(
        statusCode: 200,
        type: typeof(IEnumerable<GetDailyActivityRecordResponse>)
    )]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<
        ActionResult<IEnumerable<GetDailyActivityRecordResponse>>
    > QueryDailyActivityRecords(
        [FromQuery] int teacherId,
        [FromQuery] string? departmentName,
        [FromQuery] int? year,
        [FromQuery] string? groupName,
        [FromQuery] string? subjectName,
        [FromQuery] string? roomName,
        [FromQuery] RevenueType? revenueType,
        [FromQuery] ActivityType? activityType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate
    );

    [HttpPost("create")]
    [ProducesResponseType(statusCode: 201, type: typeof(GetDailyActivityRecordResponse))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    [ProducesResponseType(statusCode: 409, type: typeof(string))]
    public abstract Task<ActionResult<GetDailyActivityRecordResponse>> CreateDailyActivityRecord(
        [FromBody] CreateDailyActivityRecordRequest request
    );

    [HttpPut("update")]
    [ProducesResponseType(statusCode: 202, type: typeof(GetDailyActivityRecordResponse))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 409, type: typeof(string))]
    public abstract Task<ActionResult<GetDailyActivityRecordResponse>> UpdateDailyActivityRecord(
        [FromBody] UpdateDailyActivityRecordRequest request
    );

    [HttpDelete("delete/{id}")]
    [ProducesResponseType(statusCode: 204, type: typeof(GetDailyActivityRecordResponse))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<ActionResult<GetDailyActivityRecordResponse>> DeleteDailyActivityRecord(
        [FromRoute] string id
    );

    [HttpGet("monthly-summary")]
    [ProducesResponseType(
        statusCode: 200,
        type: typeof(IEnumerable<MonthlyActivitySummaryResponse>)
    )]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<
        ActionResult<IEnumerable<MonthlyActivitySummaryResponse>>
    > GetMonthlyActivitySummary([FromQuery] int teacherId);

    [HttpGet("day-statuses")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<DayStatusResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<DayStatusResponse>>> GetDailyStatuses(
        [FromQuery] int teacherId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken ct = default
    );
}
