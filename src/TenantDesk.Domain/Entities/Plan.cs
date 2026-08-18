
using System;

public class Plan
{
    public Guid PlanId { get; private set; }
    public string Name { get; private set; }= null!;
    public int RateLimitPerMinute { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Plan(){}

    public Plan(Guid planId,string name, int rateLimitPerMinute)
    {
        
        if (planId == Guid.Empty)
            throw new ArgumentException("Plan ID cannot be empty.", nameof(planId));
       
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name cannot be null or empty.", nameof(name));
      
        if (rateLimitPerMinute <= 0)
            throw new ArgumentOutOfRangeException(nameof(rateLimitPerMinute), "Rate limit must be greater than zero.");

        PlanId = planId;
        Name = name;
        RateLimitPerMinute = rateLimitPerMinute;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeRateLimit(int limit)
    {
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Rate limit must be greater than zero.");
        }
        RateLimitPerMinute = limit;
    }



}