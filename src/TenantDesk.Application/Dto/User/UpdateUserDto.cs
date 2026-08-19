
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.DTOs.Users;

// PUT /api/users/{userId}
public record UpdateUserDto(
    string Email,
    UserRole Role
);
