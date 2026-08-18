using System;

namespace TenantDesk.Domain.Entities
{
    public class Comment
    {
        public Guid CommentId { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid TicketId { get; private set; }
        public Guid UserId { get; private set; }
        public string Content { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // For EF Core
        private Comment() { }

        public Comment(Guid tenantId, Guid ticketId, Guid userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content is required.", nameof(content));

            CommentId = Guid.NewGuid();
            TenantId = tenantId;
            TicketId = ticketId;
            UserId = userId;
            Content = content;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content is required.", nameof(content));

            Content = content;
        }
    }
}