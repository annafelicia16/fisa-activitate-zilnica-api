using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.Schedules.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class SchedulesApiController : ControllerBase
{
    [HttpPost("upload")]
    [ProducesResponseType(statusCode: 201, type: typeof(UploadScheduleResponse))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 409, type: typeof(string))]
    public abstract Task<ActionResult<UploadScheduleResponse>> UploadSchedule(
        [FromForm] UploadScheduleRequest request,
        CancellationToken ct = default
    );

    [HttpGet("slots")]
    [ProducesResponseType(
        statusCode: 200,
        type: typeof(IReadOnlyList<TeacherScheduleSlotResponse>)
    )]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<
        ActionResult<IReadOnlyList<TeacherScheduleSlotResponse>>
    > GetTeacherDaySlots(
        [FromQuery] GetTeacherScheduleSlotsRequest request,
        CancellationToken ct = default
    );

    [HttpGet("teacher-day-slots-by-date")]
    [ProducesResponseType(
        statusCode: 200,
        type: typeof(IReadOnlyList<TeacherScheduleSlotResponse>)
    )]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<
        ActionResult<IReadOnlyList<TeacherScheduleSlotResponse>>
    > GetTeacherDaySlotsByDate(
        [FromQuery] GetTeacherDaySlotsByDateRequest request,
        CancellationToken ct = default
    );

    [HttpGet("by-external-teacher/{externalTeacherId}")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<ScheduleResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<ScheduleResponse>>> GetSchedulesByExternalTeacherId(
        [FromRoute] int externalTeacherId,
        CancellationToken ct = default
    );

    [HttpPost("backfill-activity-students")]
    [ProducesResponseType(statusCode: 200, type: typeof(BackfillResponse))]
    public abstract Task<ActionResult<BackfillResponse>> BackfillActivityStudents(
        CancellationToken ct = default
    );
}

public sealed record BackfillResponse(int UpdatedRows);
