using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .IsRequired(false);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TenantId);

        builder.HasIndex(c => new { c.TenantId, c.CategoryName })
            .IsUnique();
    }
}