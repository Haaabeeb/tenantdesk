using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Dto.TicketFeedback;
using TenantDesk.Application.Interfaces;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Authorize]
public class TicketFeedbacksController(ITicketFeedbackService _feedbackService) : ControllerBase
{
    // POST: api/tickets/{ticketId}/feedback
    [HttpPost("api/tickets/{ticketId:guid}/feedback")]
    public async Task<ActionResult<TicketFeedbackResponseDto>> SubmitFeedback(Guid ticketId, CreateTicketFeedbackDto dto)
    {
        var result = await _feedbackService.SubmitFeedbackAsync(ticketId, dto);
        return Ok(result);
    }

    // GET: api/tickets/{ticketId}/feedback
    [HttpGet("api/tickets/{ticketId:guid}/feedback")]
    public async Task<ActionResult<TicketFeedbackResponseDto>> GetFeedback(Guid ticketId)
    {
        var result = await _feedbackService.GetFeedbackByTicketIdAsync(ticketId);
        return Ok(result);
    }
}
