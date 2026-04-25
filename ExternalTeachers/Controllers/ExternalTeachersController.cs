using FisaActivitateZilnicaApi.ExternalTeachers.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ExternalTeachers.Models;
using FisaActivitateZilnicaApi.ExternalTeachers.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalTeachers.Controllers;

public class ExternalTeachersController(IExternalTeachersQueryService externalTeachersQueryService)
    : ExternalTeachersApiController
{
    private readonly IExternalTeachersQueryService _externalTeachersQueryService =
        externalTeachersQueryService;

    public override async Task<ActionResult<ExternalTeacher>> GetExternalTeacherByEmail(
        string email,
        CancellationToken ct = default
    )
    {
        try
        {
            ExternalTeacher externalTeacher =
                await _externalTeachersQueryService.GetExternalTeacherByEmailAsync(email, ct);

            return Ok(externalTeacher);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
