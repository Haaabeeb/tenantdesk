using AutoMapper;
using TenantDesk.Application.Dto.Category;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryResponseDto>();
    }
}