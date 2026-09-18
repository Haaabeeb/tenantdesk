using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Comment;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
public class CommentsController(CommentService _commentService) : ControllerBase
{
    // POST: api/tickets/{ticketId}/comments
    [HttpPost("api/tickets/{ticketId:guid}/comments")]
    public async Task<ActionResult<CommentResponseDto>> Create(Guid ticketId, CreateCommentDto dto)
    {
        var comment = await _commentService.CreateAsync(ticketId, dto);

        return Ok(comment);
    }

    // GET: api/tickets/{ticketId}/comments?page=1&pageSize=20
    [HttpGet("api/tickets/{ticketId:guid}/comments")]
    public async Task<ActionResult<PagedResult<CommentResponseDto>>> GetByTicket(
        Guid ticketId,
        [FromQuery] CommentFilter filter)
    {
        var result = await _commentService.GetByTicketAsync(ticketId, filter);

        return Ok(result);
    }

    // PUT: api/comments/{id}
    [HttpPut("api/comments/{id:guid}")]
    public async Task<ActionResult<CommentResponseDto>> Update(Guid id, UpdateCommentDto dto)
    {
        var comment = await _commentService.UpdateAsync(id, dto);

        return Ok(comment);
    }

    // DELETE: api/comments/{id}
    [HttpDelete("api/comments/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _commentService.DeleteAsync(id);

        return NoContent();
    }
}