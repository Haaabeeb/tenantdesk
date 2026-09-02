using AutoMapper;
using TenantDesk.Application.Dto.Sla;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping
{
    public class SlaMappingProfile : Profile
    {
        public SlaMappingProfile()
        {
            CreateMap<Sla, SlaResponseDto>();
        }
    }
}