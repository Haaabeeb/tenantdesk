using System.ComponentModel.DataAnnotations;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.User;

// PUT /api/users/{userId}
public record UpdateUserDto(
    [Required]
    [EmailAddress]
    [StringLength(256)]
    string Email,

    UserRole Role
);