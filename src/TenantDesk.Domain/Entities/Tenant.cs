
using System;

namespace TenantDesk.Domain.Entities{

public class Tenant
{

    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public string TenantName { get; private set; } = null!;
    public string Subdomain { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Plan Plan { get; private set; } = null!;

    private Tenant() { }

    public Tenant(Guid planId, string tenantName, string subdomain)
    {
        
        if (planId == Guid.Empty)
            throw new ArgumentException("Plan ID cannot be empty.", nameof(planId));

        if (string.IsNullOrWhiteSpace(tenantName))
            throw new ArgumentException("Tenant name cannot be null or empty.", nameof(tenantName));

        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain cannot be null or empty.", nameof(subdomain));

        PlanId = planId;
        TenantName = tenantName.Trim();
        Subdomain = subdomain.Trim().ToLower();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be null or empty.", nameof(name));

        TenantName = name.Trim();
        UpdateTimestamp();
    }

    public void ChangePlan(Guid planId)
    {
        if (planId == Guid.Empty)
            throw new ArgumentException("Plan ID cannot be empty.", nameof(planId));

        PlanId = planId;
        UpdateTimestamp();
    }

    public void ChangeSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain cannot be null or empty.", nameof(subdomain));

        Subdomain = subdomain.ToLowerInvariant().Trim();
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdateTimestamp();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdateTimestamp();
        }
    }
    
    private void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

}