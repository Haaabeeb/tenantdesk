namespace TenantDesk.Application.Dto.Comment;

// Used with: PUT /api/comments/{id}
public record UpdateCommentDto(
    string Content
);