namespace TenantDesk.Application.Interfaces.Identity;

public interface ICurrentTenantService
{
    Guid GetTenantId();
    string GetTenantSubdomain();
}