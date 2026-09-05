
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Services;

namespace TenantDesk.Infrastructure.BackgroundServices;

public class NotificationCleanupJob(IServiceProvider serviceProvider,ILogger<NotificationCleanupJob> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<NotificationCleanupJob> _logger = logger;
    private readonly TimeSpan _retentionPeriod = TimeSpan.FromDays(30);
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Cleanup Background Service is started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

                var cutoffDate = DateTime.UtcNow.Subtract(_retentionPeriod);
                var tenants = await tenantRepository.GetAllAsync();

                _logger.LogInformation("Running notification cleanup job. Cutoff date: {CutoffDate:u}", cutoffDate);

                foreach (var tenant in tenants)
                {
                    try
                    {
                        await notificationService.DeleteOldNotificationsAsync(tenant.TenantId, cutoffDate);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cleaning up old notifications for Tenant {TenantId}", tenant.TenantId);
                    }
                }

                _logger.LogInformation("Notification cleanup job completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during notification cleanup job execution.");
            }

            // Wait 24 hours before next execution 
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Notification Cleanup Background Service is stopping.");
    }
}