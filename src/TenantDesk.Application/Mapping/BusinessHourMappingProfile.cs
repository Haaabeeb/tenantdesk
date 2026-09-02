
using AutoMapper;
using TenantDesk.Application.Dto.BusinessHour;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;
    public class BusinessHourMappingProfile : Profile
    {
        public BusinessHourMappingProfile()
        {
            CreateMap<BusinessHour, BusinessHourResponseDto>();
        }
    }
