namespace TenantDesk.Application.Interfaces.Services;

public interface ICurrentTenantService
{
    Guid GetTenantId();

    string GetTenantSubdomain();
}