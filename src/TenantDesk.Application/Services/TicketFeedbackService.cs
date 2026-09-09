using AutoMapper;
using TenantDesk.Application.Dto.TicketFeedback;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class TicketFeedbackService : ITicketFeedbackService
{
    private readonly ITicketFeedbackRepository _feedbackRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;

    public TicketFeedbackService(
        ITicketFeedbackRepository feedbackRepository,
        ITicketRepository ticketRepository,
        ICurrentTenantService currentTenantService,
        IMapper mapper)
    {
        _feedbackRepository = feedbackRepository;
        _ticketRepository = ticketRepository;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
    }

    public async Task<TicketFeedbackResponseDto> SubmitFeedbackAsync(Guid ticketId, CreateTicketFeedbackDto dto)
    {
        var tenantId = _currentTenantService.GetTenantId();
        var ticket = await _ticketRepository.GetByIdAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");

        if (ticket.TenantId != tenantId)
            throw new UnauthorizedAccessException("Cannot access ticket from another tenant.");

        var existingFeedback = await _feedbackRepository.GetByTicketAsync(ticketId);
        if (existingFeedback != null)
            throw new InvalidOperationException("Feedback already exists for this ticket.");

        var feedback = new TicketFeedback(tenantId, ticketId, dto.Rating, dto.Comment);
        await _feedbackRepository.AddAsync(feedback);

        return _mapper.Map<TicketFeedbackResponseDto>(feedback);
    }

    public async Task<TicketFeedbackResponseDto> GetFeedbackByTicketIdAsync(Guid ticketId)
    {
        var tenantId = _currentTenantService.GetTenantId();
        var ticket = await _ticketRepository.GetByIdAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");

        if (ticket.TenantId != tenantId)
            throw new UnauthorizedAccessException("Cannot access ticket from another tenant.");

        var feedback = await _feedbackRepository.GetByTicketAsync(ticketId)
            ?? throw new KeyNotFoundException("Feedback not found for this ticket.");

        return _mapper.Map<TicketFeedbackResponseDto>(feedback);
    }
}
