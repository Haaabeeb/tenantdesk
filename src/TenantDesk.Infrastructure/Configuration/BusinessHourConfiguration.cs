using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Infrastructure.Configurations;

public class BusinessHourConfiguration : IEntityTypeConfiguration<BusinessHour>
{
    public void Configure(EntityTypeBuilder<BusinessHour> builder)
    {
        builder.ToTable("BusinessHours");

        builder.HasKey(b => b.BusinessHourId);

        builder.Property(b => b.DayOfWeek)
            .IsRequired();

        builder.Property(b => b.OpenTime)
            .IsRequired(false);

        builder.Property(b => b.CloseTime)
            .IsRequired(false);

        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.TenantId, b.DayOfWeek })
            .IsUnique();

        builder.Ignore(b => b.IsWorkingDay);
    }
}