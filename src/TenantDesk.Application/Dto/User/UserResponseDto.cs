
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Dto.User;

// Returned from: GET/POST/PUT /api/users/{userId}
public record UserResponseDto(
    Guid UserId,
    Guid TenantId,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

