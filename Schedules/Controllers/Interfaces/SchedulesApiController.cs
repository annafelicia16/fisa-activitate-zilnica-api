using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.Schedules.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class SchedulesApiController : ControllerBase
{
    [HttpPost("upload")]
    [ProducesResponseType(statusCode: 201, type: typeof(string))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<string>> UploadSchedule(UploadScheduleRequest request);

    [HttpGet("slots")]
    [ProducesResponseType(statusCode: 200, type: typeof(IReadOnlyList<TeacherScheduleSlotResponse>))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    public abstract Task<ActionResult<IReadOnlyList<TeacherScheduleSlotResponse>>> GetTeacherDaySlots(
        [FromQuery] GetTeacherScheduleSlotsRequest request,
        CancellationToken ct = default
    );
}
