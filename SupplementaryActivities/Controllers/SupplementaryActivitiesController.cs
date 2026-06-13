using FisaActivitateZilnicaApi.SupplementaryActivities.Controllers.Interfaces;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Requests;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Controllers;

public class SupplementaryActivitiesController(
    ISupplementaryActivitiesCommandService supplementaryActivitiesCommandService,
    ISupplementaryActivitiesQueryService supplementaryActivitiesQueryService,
    ISupplementaryActivityAttachmentsService supplementaryActivityAttachmentsService
) : SupplementaryActivitiesApiController
{
    public override async Task<
        ActionResult<IReadOnlyList<GetSupplementaryActivityAttachmentResponse>>
    > UploadSupplementaryActivityAttachments(
        string id,
        UploadSupplementaryActivityAttachmentsRequest request,
        CancellationToken ct = default
    )
    {
        try
        {
            IReadOnlyList<GetSupplementaryActivityAttachmentResponse> attachments =
                await supplementaryActivityAttachmentsService.UploadAttachmentsAsync(
                    id,
                    request.Files,
                    ct
                );

            return Created($"api/supplementaryActivities/{id}/attachments", attachments);
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
        ActionResult<IReadOnlyList<GetSupplementaryActivityAttachmentResponse>>
    > GetSupplementaryActivityAttachments(string id)
    {
        try
        {
            IReadOnlyList<GetSupplementaryActivityAttachmentResponse> attachments =
                await supplementaryActivityAttachmentsService.GetAttachmentsForActivityAsync(id);

            return Ok(attachments);
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

    public override async Task<IActionResult> DownloadSupplementaryActivityAttachment(
        string attachmentId
    )
    {
        try
        {
            (Stream stream, string contentType, string fileName) =
                await supplementaryActivityAttachmentsService.OpenAttachmentForDownloadAsync(
                    attachmentId
                );

            return File(stream, contentType, fileName);
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

    public override async Task<IActionResult> DeleteSupplementaryActivityAttachment(
        string attachmentId
    )
    {
        try
        {
            await supplementaryActivityAttachmentsService.DeleteAttachmentAsync(attachmentId);
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
        ActionResult<GetSupplementaryActivityResponse>
    > GetSupplementaryActivityById(string id)
    {
        try
        {
            GetSupplementaryActivityResponse activity =
                await supplementaryActivitiesQueryService.GetSupplementaryActivityByIdAsync(id);

            return Ok(activity);
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
        ActionResult<IEnumerable<GetSupplementaryActivityResponse>>
    > QuerySupplementaryActivities(int teacherId, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            IEnumerable<GetSupplementaryActivityResponse> activities =
                await supplementaryActivitiesQueryService.QuerySupplementaryActivitiesAsync(
                    teacherId,
                    startDate,
                    endDate
                );

            return Ok(activities);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<
        ActionResult<GetSupplementaryActivityResponse>
    > CreateSupplementaryActivity(CreateSupplementaryActivityRequest request)
    {
        try
        {
            GetSupplementaryActivityResponse activity =
                await supplementaryActivitiesCommandService.CreateSupplementaryActivityAsync(
                    request
                );

            return Created(
                $"api/SupplementaryActivities/get-by-id/{activity.Id}",
                activity
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public override async Task<
        ActionResult<GetSupplementaryActivityResponse>
    > UpdateSupplementaryActivity(UpdateSupplementaryActivityRequest request)
    {
        try
        {
            GetSupplementaryActivityResponse activity =
                await supplementaryActivitiesCommandService.UpdateSupplementaryActivityAsync(
                    request
                );

            return Accepted(activity);
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
        ActionResult<GetSupplementaryActivityResponse>
    > DeleteSupplementaryActivity(string id)
    {
        try
        {
            await supplementaryActivitiesCommandService.DeleteSupplementaryActivityAsync(id);
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
}
