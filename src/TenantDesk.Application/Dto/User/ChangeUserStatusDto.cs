
namespace TenantDesk.Application.DTOs.Users;

// PATCH /api/users/{userId}/status
public record ChangeUserStatusDto(
    bool IsActive
);
