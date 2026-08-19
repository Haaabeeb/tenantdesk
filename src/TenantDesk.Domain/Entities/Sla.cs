
using System;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Domain.Entities;

public class Sla
{
    public Guid SlaId { get; private set; }
    public Guid TenantId { get; private set; }
    public PriorityLevel Priority { get; private set; }
    public int ResponseTime { get; private set; }
    public int ResolutionTime { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
 
    public Tenant Tenant { get; private set; } = null!;

    private Sla() { }

    public Sla(Guid tenantId, PriorityLevel priority, int responseTime, int resolutionTime)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        if (responseTime <= 0)
            throw new ArgumentOutOfRangeException(nameof(responseTime), "Response time must be greater than zero hours.");

        if (resolutionTime <= 0)
            throw new ArgumentOutOfRangeException(nameof(resolutionTime), "Resolution time must be greater than zero hours.");

        TenantId = tenantId;
        Priority = priority;
        ResponseTime = responseTime;
        ResolutionTime = resolutionTime;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateTimes(int responseTime, int resolutionTime)
    {
        if (responseTime <= 0)
            throw new ArgumentOutOfRangeException(nameof(responseTime), "Response time must be greater than zero.");

        if (resolutionTime <= 0)
            throw new ArgumentOutOfRangeException(nameof(resolutionTime), "Resolution time must be greater than zero.");

        ResponseTime = responseTime;
        ResolutionTime = resolutionTime;
        UpdatedAt = DateTime.UtcNow;
    }
}