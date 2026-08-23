namespace TenantDesk.Application.Interfaces.Services;

public interface ISlaCalculatorService
{
    Task<DateTime> CalculateResponseDueAsync(Guid ticketId);
    Task<DateTime> CalculateResolutionDueAsync(Guid ticketId);
    Task<bool> IsBreachedAsync(Guid ticketId);
}