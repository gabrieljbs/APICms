using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Shared.DTOs.Posts;

public class CreatePostDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string Slug { get; set; } = string.Empty;

    public string? Image { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public bool IsFeatured { get; set; } = false;

    public int ReadingTime { get; set; } = 0;

    public DateTime? PublishedAt { get; set; }

    public List<Guid> TagIds { get; set; } = [];
}

public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? Slug { get; set; }
    public string? Image { get; set; }
    public PostStatus? Status { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool? IsFeatured { get; set; }
    public int? ReadingTime { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<Guid>? TagIds { get; set; }
}

public class PostResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsFeatured { get; set; }
    public int ReadingTime { get; set; }
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AuthorDto? Author { get; set; }
    public List<TagResponseDto> Tags { get; set; } = [];
}

public class AuthorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TagResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class CreateTagDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Slug { get; set; } = string.Empty;
}
