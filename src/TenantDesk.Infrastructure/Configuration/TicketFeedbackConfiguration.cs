using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Infrastructure.Configurations;

public class TicketFeedbackConfiguration : IEntityTypeConfiguration<TicketFeedback>
{
    public void Configure(EntityTypeBuilder<TicketFeedback> builder)
    {
        builder.ToTable("TicketFeedbacks");

        builder.HasKey(f => f.TicketFeedbackId);

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.Comment)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(f => f.SubmittedAt)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(f => f.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.TicketId)
            .IsUnique();

        builder.HasIndex(f => f.TenantId);
    }
}