using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.Auth;

public record AuthResponseDto(
    string Token,
    Guid UserId,
    Guid TenantId,
    string Email,
    UserRole Role,
    DateTime ExpiresAt
);