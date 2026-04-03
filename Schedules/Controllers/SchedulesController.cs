using FisaActivitateZilnicaApi.Schedules.Controllers.Interfaces;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;
using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.Schedules.Controllers;

public class SchedulesController(
    IFetImportService fetImportService,
    ISchedulesQueryService schedulesQueryService
) : SchedulesApiController
{
    public override async Task<ActionResult<string>> UploadSchedule(UploadScheduleRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("No file or empty file uploaded.");

        var ext = Path.GetExtension(request.File.FileName);
        if (!string.Equals(ext, ".fet", StringComparison.OrdinalIgnoreCase))
            return BadRequest("File must be a .fet file.");

        await using var stream = request.File.OpenReadStream();
        var result = await fetImportService.ImportAsync(
            stream,
            request.Name,
            request.Year,
            request.Semester,
            request.OddWeek
        );

        if (!result.Success)
            return result.ErrorMessage?.Contains("already exists") == true
                ? Conflict(result.ErrorMessage)
                : BadRequest(result.ErrorMessage ?? "Import failed.");

        return StatusCode(201, result.ScheduleId);
    }

    public override async Task<ActionResult<IReadOnlyList<TeacherScheduleSlotResponse>>> GetTeacherDaySlots(
        [FromQuery] GetTeacherScheduleSlotsRequest request,
        CancellationToken ct = default
    )
    {
        try
        {
            var slots = await schedulesQueryService.GetTeacherDaySlotsAsync(request, ct);
            return Ok(slots);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
