using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Auth;

public record RegisterDto(
    [Required]
    [EmailAddress]
    [StringLength(256)]
    string Email,

    [Required]
    [MinLength(8)]
    string Password,

    [Required]
    string TenantSubdomain    // used to resolve which Tenant this new user belongs to
);