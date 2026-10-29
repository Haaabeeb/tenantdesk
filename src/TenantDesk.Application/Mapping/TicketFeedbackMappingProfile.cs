using AutoMapper;
using TenantDesk.Application.Dto.TicketFeedback;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;

public class TicketFeedbackMappingProfile : Profile
{
    public TicketFeedbackMappingProfile()
    {
        CreateMap<TicketFeedback, TicketFeedbackResponseDto>();
    }
}
