using AutoMapper;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.Category;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class CategoryService(ICategoryRepository _categoryRepository, ICurrentTenantService _currentTenantService, IMapper _mapper)
{
    public async Task<CategoryResponseDto> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category with ID '{id}' not found.");

        if (category.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access categories from another tenant.");

        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<PagedResult<CategoryResponseDto>> GetFilteredAsync(CategoryFilter filter)
    {
        filter.TenantId = _currentTenantService.GetTenantId();

        var result = await _categoryRepository.GetFilteredAsync(filter);

        return new PagedResult<CategoryResponseDto>
        {
            Items = result.Items.Select(c => _mapper.Map<CategoryResponseDto>(c)).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        var tenantId = _currentTenantService.GetTenantId();

        var nameTaken = await _categoryRepository.ExistsByNameAsync(tenantId, dto.CategoryName);
        if (nameTaken)
            throw new InvalidOperationException("A category with this name already exists in this tenant.");

        var category = new Category(tenantId, dto.CategoryName, dto.Description);

        await _categoryRepository.AddAsync(category);

        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<CategoryResponseDto> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category with ID '{id}' not found.");

        if (category.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot modify categories from another tenant.");

        if (!string.Equals(category.CategoryName, dto.CategoryName, StringComparison.OrdinalIgnoreCase))
        {
            var nameTaken = await _categoryRepository.ExistsByNameAsync(category.TenantId, dto.CategoryName);
            if (nameTaken)
                throw new InvalidOperationException("A category with this name already exists in this tenant.");
        }

        category.Rename(dto.CategoryName);
        category.UpdateDescription(dto.Description);

        if (dto.IsActive)
            category.Activate();
        else
            category.Deactivate();

        await _categoryRepository.UpdateAsync(category);

        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category with ID '{id}' not found.");

        if (category.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot delete categories from another tenant.");

        var ticketCount = await _categoryRepository.GetTicketCountByCategoryAsync(id);
        if (ticketCount > 0)
            throw new InvalidOperationException("Cannot delete a category that still has tickets assigned to it.");

        await _categoryRepository.DeleteAsync(id);
    }
}