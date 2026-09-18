using TenantDesk.Application.Dto.Attachment;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;

public static class AttachmentMapperExtensions
{
    public static AttachmentResponseDto ToDto(this Attachment attachment)
    {
        return new AttachmentResponseDto(
            attachment.AttachmentId,
            attachment.TicketId,
            attachment.CommentId,
            attachment.UploadedBy,
            attachment.FileName,
            attachment.FileSize,
            attachment.CreatedAt
        );
    }
}