using TenantDesk.Application.Common;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Application.Interfaces.Persistence.Filters;

public class TicketFilter : PagedFilter
{
    public Guid TenantId { get; set; }
    public TicketStatus? Status { get; set; }
    public PriorityLevel? Priority { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? CreatedBy { get; set; }
    public bool? UnassignedOnly { get; set; }
    public string? SearchKeyword { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}