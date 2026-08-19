namespace TenantDesk.Application.Dto.Attachment;

// Used with: POST /api/tickets/{ticketId}/attachments
// TicketId comes from the route; UploadedBy comes from the authenticated user's context.
public record CreateAttachmentDto(
    Guid? CommentId,
    string StoragePath,
    string FileName,
    long FileSize
);
