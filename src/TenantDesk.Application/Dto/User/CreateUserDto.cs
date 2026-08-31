
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.User;

// Sent to: POST /api/tenants/{tenantId}/users
public record CreateUserDto(
    string Email,
    UserRole Role
);
