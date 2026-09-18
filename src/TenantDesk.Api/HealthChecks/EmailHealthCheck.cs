
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TenantDesk.Infrastructure.HealthChecks;

public class EmailHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public EmailHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var smtpConfig = _configuration.GetSection("Smtp");

        var host = smtpConfig["Host"];
        var port = int.TryParse(smtpConfig["Port"], out var p)? p: 587;

        if (string.IsNullOrWhiteSpace(host))
        {
            return HealthCheckResult.Unhealthy(
                "SMTP host is not configured.");
        }

        try
        {
            using var tcpClient = new TcpClient();

            await tcpClient.ConnectAsync(host,port,cancellationToken);

            return HealthCheckResult.Healthy(
                $"SMTP server {host}:{port} is reachable.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(
                "SMTP health check was cancelled.");
        }
        catch (SocketException ex)
        {
            return HealthCheckResult.Unhealthy(
                $"SMTP server {host}:{port} is unreachable.",
                ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SMTP health check failed.",ex);
        }
    }
}