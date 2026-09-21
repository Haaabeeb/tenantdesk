
namespace TenantDesk.Infrastructure.Settings;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = "no-reply@tenantdesk.com";

    public string FromName { get; set; } = "TenantDesk Support";

    public bool EnableSsl { get; set; } = true;
}