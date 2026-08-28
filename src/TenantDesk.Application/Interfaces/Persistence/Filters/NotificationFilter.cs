using TenantDesk.Application.Common;

namespace TenantDesk.Application.Interfaces.Persistence.Filters;

public class NotificationFilter : PagedFilter
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public bool? UnreadOnly { get; set; }
}