
namespace TenantDesk.Application.Dto.User;

// PATCH /api/users/{userId}/status
public record ChangeUserStatusDto(
    bool IsActive
);
