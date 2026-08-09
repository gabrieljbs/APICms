using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Posts;

public class CreateCommentDto
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid PostId { get; set; }

    public Guid? ParentId { get; set; }
}

public class CommentResponseDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CommentResponseDto> Replies { get; set; } = [];
}
