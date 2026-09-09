using TenantDesk.Application.Dto.TicketFeedback;

namespace TenantDesk.Application.Interfaces;

public interface ITicketFeedbackService
{
    Task<TicketFeedbackResponseDto> SubmitFeedbackAsync(Guid ticketId, CreateTicketFeedbackDto dto);
    Task<TicketFeedbackResponseDto> GetFeedbackByTicketIdAsync(Guid ticketId);
}
