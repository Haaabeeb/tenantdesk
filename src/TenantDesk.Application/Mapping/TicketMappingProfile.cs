using AutoMapper;
using TenantDesk.Application.Dto.Ticket;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;

public class TicketMappingProfile : Profile
{
    public TicketMappingProfile()
    {
        CreateMap<Ticket, TicketResponseDto>();
    }
}