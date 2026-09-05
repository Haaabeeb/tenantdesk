
using AutoMapper;
using TenantDesk.Application.Dto.Notification;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationResponseDto>();
    }
}