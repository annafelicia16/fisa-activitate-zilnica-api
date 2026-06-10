using FisaActivitateZilnicaApi.Schedules.Controllers.Interfaces;
using FisaActivitateZilnicaApi.Schedules.DTOs.Requests;
using FisaActivitateZilnicaApi.Schedules.DTOs.Responses;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.Schedules.Controllers;

public class SchedulesController(
    IFetImportService fetImportService,
    ISchedulesQueryService schedulesQueryService
) : SchedulesApiController
{
    public override async Task<ActionResult<UploadScheduleResponse>> UploadSchedule(
        [FromForm] UploadScheduleRequest request,
        CancellationToken ct = default
    )
    {
        if (request.OddWeekFile == null || request.OddWeekFile.Length == 0)
            return BadRequest("Odd week file is required.");

        if (request.EvenWeekFile == null || request.EvenWeekFile.Length == 0)
            return BadRequest("Even week file is required.");

        if (!HasFetExtension(request.OddWeekFile))
            return BadRequest("Odd week file must be a .fet file.");

        if (!HasFetExtension(request.EvenWeekFile))
            return BadRequest("Even week file must be a .fet file.");

        await using var oddWeekStream = request.OddWeekFile.OpenReadStream();
        await using var evenWeekStream = request.EvenWeekFile.OpenReadStream();
        var result = await fetImportService.ImportAsync(
            oddWeekStream,
            evenWeekStream,
            request.Name,
            request.Year,
            request.Semester,
            request.StartDate,
            request.EndDate,
            ct
        );

        if (!result.Success)
            return result.ErrorMessage?.Contains("already exists") == true
                ? Conflict(result.ErrorMessage)
                : BadRequest(result.ErrorMessage ?? "Import failed.");

        return StatusCode(
            201,
            new UploadScheduleResponse(
                result.ScheduleYearId!.Value,
                result.ScheduleSemesterId!.Value,
                result.OddWeekScheduleId!.Value,
                result.EvenWeekScheduleId!.Value
            )
        );
    }

    public override async Task<
        ActionResult<IReadOnlyList<TeacherScheduleSlotResponse>>
    > GetTeacherDaySlots(
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

    public override async Task<
        ActionResult<IReadOnlyList<TeacherScheduleSlotResponse>>
    > GetTeacherDaySlotsByDate(
        [FromQuery] GetTeacherDaySlotsByDateRequest request,
        CancellationToken ct = default
    )
    {
        try
        {
            var slots = await schedulesQueryService.GetTeacherDaySlotsByDateAsync(request, ct);
            return Ok(slots);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<
        ActionResult<IReadOnlyList<ScheduleResponse>>
    > GetSchedulesByExternalTeacherId(int externalTeacherId, CancellationToken ct = default)
    {
        try
        {
            var schedules = await schedulesQueryService.GetSchedulesByExternalTeacherIdAsync(
                externalTeacherId,
                ct
            );

            if (!schedules.Any())
                return NotFound(
                    $"No schedules found for external teacher id '{externalTeacherId}'."
                );

            return Ok(schedules);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<ActionResult<BackfillResponse>> BackfillActivityStudents(
        CancellationToken ct = default
    )
    {
        int updated = await schedulesQueryService.BackfillActivityStudentsCommentRefsAsync(ct);
        return Ok(new BackfillResponse(updated));
    }

    public override async Task<ActionResult<BackfillResponse>> BackfillActivityStudentsAgsisNames(
        CancellationToken ct = default
    )
    {
        int updated = await schedulesQueryService.BackfillActivityStudentsAgsisNamesAsync(ct);
        return Ok(new BackfillResponse(updated));
    }

    public override async Task<ActionResult<IReadOnlyList<ScheduleResponse>>> GetAllSchedules(
        CancellationToken ct = default
    )
    {
        var schedules = await schedulesQueryService.GetAllSchedulesAsync(ct);
        return Ok(schedules);
    }

    private static bool HasFetExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        return string.Equals(ext, ".fet", StringComparison.OrdinalIgnoreCase);
    }
}
