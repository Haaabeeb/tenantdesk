using TenantDesk.Application.Common;

namespace TenantDesk.Application.Interfaces.Persistence.Filters;

public class CommentFilter : PagedFilter
{
    public Guid TenantId { get; set; }
    public Guid? TicketId { get; set; }
    public Guid? UserId { get; set; }
}