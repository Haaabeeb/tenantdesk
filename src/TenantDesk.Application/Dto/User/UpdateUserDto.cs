
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.User;

// PUT /api/users/{userId}
public record UpdateUserDto(
    string Email,
    UserRole Role
);
