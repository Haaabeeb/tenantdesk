namespace TenantDesk.Application.Dto.Category;

// Used with: PUT /api/categories/{categoryId}
public record UpdateCategoryDto(
    string CategoryName,
    string? Description,
    bool IsActive
);
