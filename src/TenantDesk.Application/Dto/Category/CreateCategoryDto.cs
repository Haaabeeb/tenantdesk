namespace TenantDesk.Application.Dto.Category;

// Used with: POST /api/categories
public record CreateCategoryDto(
    string CategoryName,
    string? Description
);
