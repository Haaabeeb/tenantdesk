using System.ComponentModel.DataAnnotations;

public record LoginDto(
    [Required]
    [EmailAddress]
    [StringLength(256)]
    string Email,

    [Required]
    string Password,

    [Required]
    string TenantSubdomain
);