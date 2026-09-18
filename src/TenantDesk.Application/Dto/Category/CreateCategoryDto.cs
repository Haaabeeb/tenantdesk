using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Category;

// Used with: POST /api/categories
public record CreateCategoryDto(
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 150 characters.")]
    string CategoryName,

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    string? Description
);
