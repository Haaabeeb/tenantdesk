using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Infrastructure.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("Tenants");

            builder.HasKey(t => t.TenantId);

            builder.Property(t => t.TenantName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(t => t.Subdomain)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(t => t.Subdomain)
                .IsUnique();

            builder.Property(t => t.IsActive)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(t => t.Plan)
                .WithMany()
                .HasForeignKey(t => t.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}