using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Attachment;

// Used with: POST /api/tickets/{ticketId}/attachments
// TicketId comes from the route; UploadedBy comes from the authenticated user's context.
// StoragePath is generated server-side, not client-supplied.
public record CreateAttachmentDto(
    Guid? CommentId,

    [Required(ErrorMessage = "File name is required.")]
    [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters.")]
    string FileName,

    [Range(1, 52428800, ErrorMessage = "File size must be greater than 0 bytes and at most 50 MB.")]
    long FileSize
);