using FisaActivitateZilnicaApi.ScheduleCatalog.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using FisaActivitateZilnicaApi.ScheduleCatalog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Controllers;

public class ScheduleCatalogController(IScheduleCatalogQueryService scheduleCatalogQueryService)
    : ScheduleCatalogApiController
{
    public override async Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetFaculties(
        int externalTeacherId,
        CancellationToken ct = default
    )
    {
        try
        {
            return Ok(await scheduleCatalogQueryService.GetFacultiesAsync(externalTeacherId, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetSpecializations(
        int externalTeacherId,
        int facultyId,
        CancellationToken ct = default
    )
    {
        try
        {
            return Ok(
                await scheduleCatalogQueryService.GetSpecializationsAsync(
                    externalTeacherId,
                    facultyId,
                    ct
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<ActionResult<IReadOnlyList<CatalogYearResponse>>> GetYears(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        CancellationToken ct = default
    )
    {
        try
        {
            return Ok(
                await scheduleCatalogQueryService.GetYearsAsync(
                    externalTeacherId,
                    facultyId,
                    specializationId,
                    ct
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<ActionResult<IReadOnlyList<CatalogGroupResponse>>> GetGroups(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        CancellationToken ct = default
    )
    {
        try
        {
            return Ok(
                await scheduleCatalogQueryService.GetGroupsAsync(
                    externalTeacherId,
                    facultyId,
                    specializationId,
                    year,
                    ct
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<ActionResult<IReadOnlyList<CatalogItemResponse>>> GetSubjects(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        CancellationToken ct = default
    )
    {
        try
        {
            return Ok(
                await scheduleCatalogQueryService.GetSubjectsAsync(
                    externalTeacherId,
                    facultyId,
                    specializationId,
                    year,
                    groupKey,
                    ct
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<ActionResult<IReadOnlyList<CatalogRoomResponse>>> GetRooms(
        int externalTeacherId,
        int facultyId,
        int specializationId,
        int year,
        string groupKey,
        int subjectId,
        CancellationToken ct = default
    )
    {
        try
        {
            return Ok(
                await scheduleCatalogQueryService.GetRoomsAsync(
                    externalTeacherId,
                    facultyId,
                    specializationId,
                    year,
                    groupKey,
                    subjectId,
                    ct
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
