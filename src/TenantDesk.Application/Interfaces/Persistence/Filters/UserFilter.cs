using TenantDesk.Application.Common;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces.Persistence;

public class UserFilter : PagedFilter
{
    public Guid TenantId { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchKeyword { get; set; }
}