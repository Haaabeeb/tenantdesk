namespace TenantDesk.Application.Common;

public abstract class PagedFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}