
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.DTOs.Users;

// Sent to: POST /api/tenants/{tenantId}/users
public record CreateUserDto(
    string Email,
    string? MicrosoftUserId,
    UserRole Role
);
