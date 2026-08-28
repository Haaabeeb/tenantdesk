using TenantDesk.Application.Common;

namespace TenantDesk.Application.Interfaces.Persistence.Filters;

public class FeedbackFilter : PagedFilter
{
    public Guid TenantId { get; set; }
    public int? MaxRating { get; set; }
}