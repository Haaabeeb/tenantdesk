using System;

namespace TenantDesk.Domain.Entities
{
    public class Attachment
    {
        public Guid AttachmentId { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid TicketId { get; private set; }
        public Guid? CommentId { get; private set; }
        public Guid UploadedBy { get; private set; }
        public string StoragePath { get; private set; }
        public string FileName { get; private set; }
        public long FileSize { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // For EF Core
        private Attachment() { }

        public Attachment(Guid tenantId, Guid ticketId, Guid? commentId, Guid uploadedBy,
                           string storagePath, string fileName, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
                throw new ArgumentException("Storage path is required.", nameof(storagePath));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.", nameof(fileName));

            if (fileSize <= 0)
                throw new ArgumentException("File size must be greater than 0.", nameof(fileSize));

            AttachmentId = Guid.NewGuid();
            TenantId = tenantId;
            TicketId = ticketId;
            CommentId = commentId;
            UploadedBy = uploadedBy;
            StoragePath = storagePath;
            FileName = fileName;
            FileSize = fileSize;
            CreatedAt = DateTime.UtcNow;
        }
    }
}