namespace TenantDesk.Application.Dto.Attachment;

// Returned from: GET/POST /api/tickets/{ticketId}/attachments
public record AttachmentResponseDto(
    Guid AttachmentId,
    Guid TicketId,
    Guid? CommentId,
    Guid UploadedBy,
    string FileName,
    long FileSize,
    DateTime CreatedAt
);
