using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Application.Models;

namespace TenantDesk.Infrastructure.BackgroundServices;

public sealed class EmailDispatchWorker(
    IEmailJobQueue _queue,
    IServiceProvider _serviceProvider,
    ILogger<EmailDispatchWorker> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailDispatchWorker started — waiting for jobs.");

       await foreach (var job in ReadAllJobsAsync(stoppingToken))
        {
            await ProcessJobAsync(job, stoppingToken);
        }

        _logger.LogInformation("EmailDispatchWorker stopped.");
    }

    private async IAsyncEnumerable<EmailJob> ReadAllJobsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            EmailJob job;
            try
            {
                job = await _queue.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }

            yield return job;
        }
    }

    private async Task ProcessJobAsync(EmailJob job, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            _logger.LogInformation(
                "Processing email job. TicketId: {TicketId}, TenantId: {TenantId}, To: {To}, EnqueuedAt: {EnqueuedAt:u}",
                job.TicketId,
                job.TenantId,
                job.To,
                job.EnqueuedAt);

            await emailService.SendAsync(job.To, job.Subject, job.Body);

            _logger.LogInformation(
                "Email job completed successfully. TicketId: {TicketId}, TenantId: {TenantId}, To: {To}",
                job.TicketId,
                job.TenantId,
                job.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process email job. TicketId: {TicketId}, TenantId: {TenantId}, To: {To}. The worker will continue processing remaining jobs.",
                job.TicketId,
                job.TenantId,
                job.To);
        }
    }
}
