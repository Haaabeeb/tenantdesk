using System.Threading.Channels;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Application.Models;

namespace TenantDesk.Infrastructure.BackgroundServices;


public sealed class EmailJobQueue : IEmailJobQueue
{
    private readonly Channel<EmailJob> _channel = Channel.CreateBounded<EmailJob>(
        new BoundedChannelOptions(1_000)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

    public ValueTask EnqueueAsync(EmailJob job, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(job, cancellationToken);

    public ValueTask<EmailJob> ReadAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAsync(cancellationToken);
}
