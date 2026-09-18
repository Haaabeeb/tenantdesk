using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Attachment;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
public class AttachmentsController(AttachmentService _attachmentService) : ControllerBase
{
    // POST: api/tickets/{ticketId}/attachments
    [HttpPost("api/tickets/{ticketId:guid}/attachments")]
    public async Task<ActionResult<AttachmentResponseDto>> Create(
        Guid ticketId,
        [FromForm] CreateAttachmentDto dto,
        IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        var attachment = await _attachmentService.CreateAsync(ticketId, dto, stream);

        return Ok(attachment);
    }

    // GET: api/tickets/{ticketId}/attachments?page=1&pageSize=20
    [HttpGet("api/tickets/{ticketId:guid}/attachments")]
    public async Task<ActionResult<PagedResult<AttachmentResponseDto>>> GetByTicket(
        Guid ticketId,
        [FromQuery] AttachmentFilter filter)
    {
        var result = await _attachmentService.GetByTicketAsync(ticketId, filter);

        return Ok(result);
    }

    // DELETE: api/attachments/{id}
    [HttpDelete("api/attachments/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _attachmentService.DeleteAsync(id);

        return NoContent();
    }
}