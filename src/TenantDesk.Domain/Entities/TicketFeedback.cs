using System;

namespace TenantDesk.Domain.Entities
{
    public class TicketFeedback
    {
        public Guid TicketFeedbackId { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid TicketId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }
        public DateTime SubmittedAt { get; private set; }

        // For EF Core
        private TicketFeedback() { }

        public TicketFeedback(Guid tenantId, Guid ticketId, int rating, string? comment)
        {
            ValidateRating(rating);

            TicketFeedbackId = Guid.NewGuid();
            TenantId = tenantId;
            TicketId = ticketId;
            Rating = rating;
            Comment = comment;
            SubmittedAt = DateTime.UtcNow;
        }

        public void UpdateFeedback(int rating, string? comment)
        {
            ValidateRating(rating);

            Rating = rating;
            Comment = comment;
        }

        private static void ValidateRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        }
    }
}