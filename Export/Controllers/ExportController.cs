using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;
using FisaActivitateZilnicaApi.ExternalTeachers.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalTeachers.Services.Interfaces;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.Export.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController(
    IExternalTeachersQueryService externalTeachersQueryService,
    IDailyActivityRecordsQueryService dailyActivityRecordsQueryService,
    ISupplementaryActivitiesQueryService supplementaryActivitiesQueryService
) : ControllerBase
{
    [HttpGet("monthly-pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportMonthlyFisaActivitatePdf(
        [FromQuery] int teacherId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct = default
    )
    {
        if (teacherId <= 0)
        {
            return BadRequest("Query parameter 'teacherId' must be greater than 0.");
        }
        if (year < 1900 || year > 9999)
        {
            return BadRequest("Query parameter 'year' is out of range.");
        }
        if (month < 1 || month > 12)
        {
            return BadRequest("Query parameter 'month' must be between 1 and 12.");
        }

        TeacherProfileResponse teacher;
        try
        {
            teacher = await externalTeachersQueryService.GetTeacherProfileAsync(teacherId, ct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        DateTime firstDayOfMonth = new(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        DateTime firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

        IEnumerable<GetDailyActivityRecordResponse> recordsEnum =
            await dailyActivityRecordsQueryService.QueryDailyActivityRecordsAsync(
                teacherId: teacherId,
                departmentName: null,
                year: null,
                groupName: null,
                subjectName: null,
                roomName: null,
                revenueType: null,
                activityType: null,
                startDate: firstDayOfMonth,
                endDate: firstDayOfNextMonth
            );

        List<GetDailyActivityRecordResponse> records = recordsEnum
            .OrderBy(r => r.StartDate)
            .ToList();

        IEnumerable<GetSupplementaryActivityResponse> supplementaryEnum =
            await supplementaryActivitiesQueryService.QuerySupplementaryActivitiesAsync(
                teacherId,
                firstDayOfMonth,
                firstDayOfNextMonth
            );

        List<GetSupplementaryActivityResponse> supplementary = supplementaryEnum.ToList();

        byte[] pdfBytes = FisaActivitateDocument.Render(
            new FisaActivitateInput(teacher, year, month, records, supplementary)
        );

        string filename = $"Fisa_Activitate_{year}_{month:00}_{teacherId}.pdf";
        return File(pdfBytes, "application/pdf", filename);
    }
}
