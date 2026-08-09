using Shared.Enums;

namespace Blog.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Image { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsFeatured { get; set; } = false;
    public int ReadingTime { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public ICollection<PostTag> PostTags { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
