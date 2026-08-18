using System;

namespace TenantDesk.Domain.Entities;

public class Plan
{
    public Guid PlanId { get; private set; }

    public string Name { get; private set; } = null!;

    public int RateLimitPerMinute { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Plan() { }

    public Plan(string name, int rateLimitPerMinute)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name cannot be null or empty.",nameof(name));

        if (rateLimitPerMinute <= 0)
            throw new ArgumentOutOfRangeException(nameof(rateLimitPerMinute),"Rate limit must be greater than zero.");

        Name = name.Trim();
        RateLimitPerMinute = rateLimitPerMinute;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeRateLimit(int limit)
    {
        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit),"Rate limit must be greater than zero.");

        RateLimitPerMinute = limit;
    }
}