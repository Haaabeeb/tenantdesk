using AutoMapper;
using TenantDesk.Application.Dto.User;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponseDto>();
    }
}   