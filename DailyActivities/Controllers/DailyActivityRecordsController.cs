using FisaActivitateZilnicaApi.DailyActivities.Controllers.Interfaces;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.DailyActivities.Controllers;

public class DailyActivityRecordsController(
    IDailyActivityRecordsCommandService dailyActivityRecordsCommandService,
    IDailyActivityRecordsQueryService dailyActivityRecordsQueryService
) : DailyActivityRecordsApiController
{
    private readonly IDailyActivityRecordsCommandService _dailyActivityRecordsCommandService =
        dailyActivityRecordsCommandService;
    private readonly IDailyActivityRecordsQueryService _dailyActivityRecordsQueryService =
        dailyActivityRecordsQueryService;

    public override async Task<
        ActionResult<GetDailyActivityRecordResponse>
    > GetDailyActivityRecordById(string id)
    {
        try
        {
            GetDailyActivityRecordResponse dailyActivityRecord =
                await _dailyActivityRecordsQueryService.GetDailyActivityRecordByIdAsync(id);

            return Ok(dailyActivityRecord);
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

    public override async Task<
        ActionResult<IEnumerable<GetDailyActivityRecordResponse>>
    > QueryDailyActivityRecords(
        int teacherId,
        string? departmentName,
        int? year,
        string? groupName,
        string? subjectName,
        string? roomName,
        RevenueType? revenueType,
        ActivityType? activityType,
        DateTime? startDate,
        DateTime? endDate
    )
    {
        try
        {
            IEnumerable<GetDailyActivityRecordResponse> dailyActivityRecords =
                await _dailyActivityRecordsQueryService.QueryDailyActivityRecordsAsync(
                    teacherId,
                    departmentName,
                    year,
                    groupName,
                    subjectName,
                    roomName,
                    revenueType,
                    activityType,
                    startDate,
                    endDate
                );

            return Ok(dailyActivityRecords);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<
        ActionResult<GetDailyActivityRecordResponse>
    > CreateDailyActivityRecord(CreateDailyActivityRecordRequest request)
    {
        try
        {
            GetDailyActivityRecordResponse dailyActivityRecord =
                await _dailyActivityRecordsCommandService.CreateDailyActivityRecordAsync(request);

            return Created(
                $"api/dailyActivityRecords/get-by-id/{dailyActivityRecord.Id}",
                dailyActivityRecord
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<
        ActionResult<AutoFillDailyActivityRecordsResponse>
    > AutoFillDailyActivityRecords(
        AutoFillDailyActivityRecordsRequest request,
        CancellationToken ct = default
    )
    {
        try
        {
            AutoFillDailyActivityRecordsResponse result =
                await _dailyActivityRecordsCommandService.AutoFillDailyActivityRecordsAsync(
                    request,
                    ct
                );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<
        ActionResult<GetDailyActivityRecordResponse>
    > UpdateDailyActivityRecord(UpdateDailyActivityRecordRequest request)
    {
        try
        {
            GetDailyActivityRecordResponse dailyActivityRecord =
                await _dailyActivityRecordsCommandService.UpdateDailyActivityRecordAsync(request);

            return Accepted(dailyActivityRecord);
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

    public override async Task<
        ActionResult<GetDailyActivityRecordResponse>
    > DeleteDailyActivityRecord(string id)
    {
        try
        {
            await _dailyActivityRecordsCommandService.DeleteDailyActivityRecordAsync(id);
            return NoContent();
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

    public override async Task<
        ActionResult<IEnumerable<MonthlyActivitySummaryResponse>>
    > GetMonthlyActivitySummary(int teacherId)
    {
        try
        {
            IEnumerable<MonthlyActivitySummaryResponse> summaries =
                await _dailyActivityRecordsQueryService.GetMonthlySummariesAsync(teacherId);

            return Ok(summaries);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<ActionResult<IReadOnlyList<DayStatusResponse>>> GetDailyStatuses(
        int teacherId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default
    )
    {
        try
        {
            IReadOnlyList<DayStatusResponse> statuses =
                await _dailyActivityRecordsQueryService.GetDailyStatusesAsync(
                    teacherId,
                    startDate,
                    endDate,
                    ct
                );
            return Ok(statuses);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
