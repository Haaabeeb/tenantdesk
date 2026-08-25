using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Infrastructure.Configurations;

public class SlaConfiguration : IEntityTypeConfiguration<Sla>
{
    public void Configure(EntityTypeBuilder<Sla> builder)
    {
        builder.ToTable("Slas");

        builder.HasKey(s => s.SlaId);

        builder.Property(s => s.Priority)
            .IsRequired();

        builder.Property(s => s.ResponseTime)
            .IsRequired();

        builder.Property(s => s.ResolutionTime)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.TenantId, s.Priority })
            .IsUnique();
    }
}