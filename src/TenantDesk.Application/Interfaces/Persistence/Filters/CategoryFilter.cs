using TenantDesk.Application.Common;

namespace TenantDesk.Application.Interfaces.Persistence;

public class CategoryFilter : PagedFilter
{
    public Guid TenantId { get; set; }
    public bool? IsActive { get; set; }
}