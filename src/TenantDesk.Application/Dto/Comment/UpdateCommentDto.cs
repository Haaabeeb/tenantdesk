using System.ComponentModel.DataAnnotations;

namespace TenantDesk.Application.Dto.Comment;

// Used with: PUT /api/comments/{id}
public record UpdateCommentDto(
    [Required(ErrorMessage = "Comment content is required.")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Comment content cannot exceed 4000 characters.")]
    string Content
);