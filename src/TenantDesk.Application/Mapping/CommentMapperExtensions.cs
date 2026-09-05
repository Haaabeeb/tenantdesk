using TenantDesk.Application.Dto.Comment;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Mapping;

public static class CommentMapperExtensions
{
    public static CommentResponseDto ToDto(this Comment comment)
    {
        return new CommentResponseDto(
            comment.CommentId,
            comment.TicketId,
            comment.UserId,
            comment.Content,
            comment.CreatedAt
        );
    }
}