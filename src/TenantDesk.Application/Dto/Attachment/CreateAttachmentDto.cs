namespace TenantDesk.Application.Dto.Attachment;

// Used with: POST /api/tickets/{ticketId}/attachments
// TicketId comes from the route; UploadedBy comes from the authenticated user's context.
// StoragePath is generated server-side, not client-supplied.
public record CreateAttachmentDto(
    Guid? CommentId,
    string FileName,
    long FileSize
);