using TenantDesk.Application.Common;

namespace TenantDesk.Application.Interfaces.Persistence.Filters;

public class AttachmentFilter : PagedFilter
{
    public Guid? TicketId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? UploadedBy { get; set; }
}