namespace TenantDesk.Application.Dto.Category;

// Returned from: GET/POST/PUT /api/categories
public record CategoryResponseDto(
    Guid CategoryId,
    string CategoryName,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);
