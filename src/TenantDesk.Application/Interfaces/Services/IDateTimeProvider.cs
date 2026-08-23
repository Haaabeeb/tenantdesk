namespace TenantDesk.Application.Interfaces.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Today { get; }
}