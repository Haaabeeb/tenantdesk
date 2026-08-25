using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Infrastructure.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(p => p.PlanId);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.RateLimitPerMinute)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}