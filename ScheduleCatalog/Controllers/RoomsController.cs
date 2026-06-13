using FisaActivitateZilnicaApi.ScheduleCatalog.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ScheduleCatalog.DTOs.Responses;
using FisaActivitateZilnicaApi.ScheduleCatalog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ScheduleCatalog.Controllers;

public class RoomsController(IScheduleCatalogQueryService scheduleCatalogQueryService)
    : RoomsApiController
{
    private readonly IScheduleCatalogQueryService _scheduleCatalogQueryService =
        scheduleCatalogQueryService;

    public override async Task<ActionResult<IReadOnlyList<CatalogRoomResponse>>> GetRooms(
        string? faculty,
        string? specialization,
        int? year,
        string? group,
        string? subject,
        string? search,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<CatalogRoomResponse> rooms =
            await _scheduleCatalogQueryService.SearchRoomsAsync(
                faculty,
                specialization,
                year,
                group,
                subject,
                search,
                ct
            );

        return Ok(rooms);
    }
}
