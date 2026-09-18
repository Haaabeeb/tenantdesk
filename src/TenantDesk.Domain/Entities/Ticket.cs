using System;

namespace TenantDesk.Domain.Entities
{
    public class Ticket
    {
        public Guid TicketId { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid CategoryId { get; private set; }
        public Guid SlaId { get; private set; }
        //navigation property 
        public Sla Sla { get; private set; } = null!;
        public Guid CreatedBy { get; private set; }
        public Guid? AssignedTo { get; private set; }
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public TicketStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // For EF Core
        private Ticket() { }

        public Ticket(Guid tenantId, Guid categoryId, Guid slaId, Guid createdBy,
                       string title, string? description)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            TicketId = Guid.NewGuid();
            TenantId = tenantId;
            CategoryId = categoryId;
            SlaId = slaId;
            CreatedBy = createdBy;
            Title = title;
            Description = description;
            Status = TicketStatus.Open;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignTo(Guid userId)
        {
            AssignedTo = userId;
            Status = TicketStatus.Assigned;
            Touch();
        }

        public void Unassign()
        {
            AssignedTo = null;
            Status = TicketStatus.Open;
            Touch();
        }

        public void UpdateDetails(string title, string? description)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            Title = title;
            Description = description;
            Touch();
        }

        public void ChangeStatus(TicketStatus newStatus)
        {
            if (Status == TicketStatus.Closed)
                throw new InvalidOperationException("Cannot change status of a closed ticket.");

            Status = newStatus;
            Touch();
        }

        public void Close()
        {
            if (Status == TicketStatus.Closed)
                throw new InvalidOperationException("Ticket is already closed.");

            Status = TicketStatus.Closed;
            Touch();
        }

        private void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

}


