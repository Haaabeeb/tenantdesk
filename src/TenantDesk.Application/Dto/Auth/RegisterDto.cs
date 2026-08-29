namespace TenantDesk.Application.Dto.Auth;

public record RegisterDto(
    string Email,
    string Password,
    string TenantSubdomain    // used to resolve which Tenant this new user belongs to
);