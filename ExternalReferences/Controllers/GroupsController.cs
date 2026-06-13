using FisaActivitateZilnicaApi.ExternalReferences.Controllers.Interfaces;
using FisaActivitateZilnicaApi.ExternalReferences.DTOs.Responses;
using FisaActivitateZilnicaApi.ExternalReferences.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.ExternalReferences.Controllers;

public class GroupsController(IGroupsQueryService groupsQueryService) : GroupsApiController
{
    private readonly IGroupsQueryService _groupsQueryService = groupsQueryService;

    public override async Task<ActionResult<IReadOnlyList<GroupResponse>>> GetGroups(
        string? faculty,
        string? specialization,
        int? year,
        string? search,
        CancellationToken ct = default
    )
    {
        IReadOnlyList<GroupResponse> groups = await _groupsQueryService.SearchGroupsAsync(
            faculty,
            specialization,
            year,
            search,
            ct
        );

        return Ok(groups);
    }
}
