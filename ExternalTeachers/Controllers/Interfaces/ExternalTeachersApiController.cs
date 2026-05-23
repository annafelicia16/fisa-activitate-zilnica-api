using FisaActivitateZilnicaApi.ExternalTeachers.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalTeachers.Models;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Controllers.Interfaces;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ExternalTeachersApiController : ControllerBase
{
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(statusCode: 200, type: typeof(ExternalTeacher))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<ActionResult<ExternalTeacher>> GetExternalTeacherByEmail(
        [FromRoute] string email,
        CancellationToken ct = default
    );

    [HttpGet("{externalTeacherId:int}/profile")]
    [ProducesResponseType(statusCode: 200, type: typeof(TeacherProfileResponse))]
    [ProducesResponseType(statusCode: 400, type: typeof(string))]
    [ProducesResponseType(statusCode: 404, type: typeof(string))]
    public abstract Task<ActionResult<TeacherProfileResponse>> GetTeacherProfile(
        [FromRoute] int externalTeacherId,
        CancellationToken ct = default
    );
}
