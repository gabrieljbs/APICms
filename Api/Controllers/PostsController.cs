using System.Security.Claims;
using Blog;
using Blog.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Posts;
using Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("posts")]
public class PostsController(BlogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponseDto<PostResponseDto>>> GetPublished([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        page = page < 1 ? 1 : page;
        limit = limit < 1 ? 10 : limit;

        var query = dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.Status == PostStatus.Published);

        var total = await query.CountAsync();
        var rawPosts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var posts = rawPosts.Select(MapToDto).ToList();

        return Ok(new PaginatedResponseDto<PostResponseDto>
        {
            Data = posts,
            Meta = new PaginationMeta
            {
                Page = page,
                Limit = limit,
                Total = total,
                LastPage = (int)Math.Ceiling(total / (double)limit)
            }
        });
    }

    [Authorize]
    [HttpGet("cms")]
    public async Task<ActionResult<PaginatedResponseDto<PostResponseDto>>> GetAllCMS([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        page = page < 1 ? 1 : page;
        limit = limit < 1 ? 10 : limit;

        var query = dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag);

        var total = await query.CountAsync();
        var rawPosts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var posts = rawPosts.Select(MapToDto).ToList();

        return Ok(new PaginatedResponseDto<PostResponseDto>
        {
            Data = posts,
            Meta = new PaginationMeta
            {
                Page = page,
                Limit = limit,
                Total = total,
                LastPage = (int)Math.Ceiling(total / (double)limit)
            }
        });
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponseDto<PostResponseDto>>> GetBySlug(string slug)
    {
        var post = await dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PostStatus.Published);

        if (post == null)
            return NotFound(ApiResponseDto<PostResponseDto>.Error("Post nao encontrado."));

        return Ok(ApiResponseDto<PostResponseDto>.Success(MapToDto(post)));
    }

    [Authorize]
    [HttpGet("cms/detail/{slug}")]
    public async Task<ActionResult<ApiResponseDto<PostResponseDto>>> GetBySlugCMS(string slug)
    {
        var post = await dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (post == null)
            return NotFound(ApiResponseDto<PostResponseDto>.Error("Post nao encontrado."));

        return Ok(ApiResponseDto<PostResponseDto>.Success(MapToDto(post)));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<PostResponseDto>>> Create([FromBody] CreatePostDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var authorId))
            return Unauthorized();

        var post = new Post
        {
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            Slug = dto.Slug,
            Image = dto.Image,
            Status = dto.Status,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            IsFeatured = dto.IsFeatured,
            ReadingTime = dto.ReadingTime,
            PublishedAt = dto.PublishedAt,
            AuthorId = authorId
        };

        if (dto.TagIds.Count > 0)
        {
            post.PostTags = dto.TagIds.Select(tagId => new PostTag { PostId = post.Id, TagId = tagId }).ToList();
        }

        dbContext.Posts.Add(post);
        await dbContext.SaveChangesAsync();

        var createdPost = await dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstAsync(p => p.Id == post.Id);

        return Ok(ApiResponseDto<PostResponseDto>.Success(MapToDto(createdPost), "Post criado com sucesso."));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponseDto<PostResponseDto>>> Update(Guid id, [FromBody] UpdatePostDto dto)
    {
        var post = await dbContext.Posts
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound(ApiResponseDto<PostResponseDto>.Error("Post nao encontrado."));

        if (dto.Title != null) post.Title = dto.Title;
        if (dto.Description != null) post.Description = dto.Description;
        if (dto.Content != null) post.Content = dto.Content;
        if (dto.Slug != null) post.Slug = dto.Slug;
        if (dto.Image != null) post.Image = dto.Image;
        if (dto.Status.HasValue) post.Status = dto.Status.Value;
        if (dto.MetaTitle != null) post.MetaTitle = dto.MetaTitle;
        if (dto.MetaDescription != null) post.MetaDescription = dto.MetaDescription;
        if (dto.IsFeatured.HasValue) post.IsFeatured = dto.IsFeatured.Value;
        if (dto.ReadingTime.HasValue) post.ReadingTime = dto.ReadingTime.Value;
        if (dto.PublishedAt.HasValue) post.PublishedAt = dto.PublishedAt.Value;

        if (dto.TagIds != null)
        {
            dbContext.PostTags.RemoveRange(post.PostTags);
            post.PostTags = dto.TagIds.Select(tagId => new PostTag { PostId = post.Id, TagId = tagId }).ToList();
        }

        post.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var updatedPost = await dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstAsync(p => p.Id == post.Id);

        return Ok(ApiResponseDto<PostResponseDto>.Success(MapToDto(updatedPost), "Post atualizado com sucesso."));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var post = await dbContext.Posts.FindAsync(id);
        if (post == null)
            return NotFound(ApiResponseDto<bool>.Error("Post nao encontrado."));

        dbContext.Posts.Remove(post);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.Success(true, "Post removido com sucesso."));
    }

    [HttpPost("{slug}/view")]
    public async Task<ActionResult<ApiResponseDto<int>>> RegisterView(string slug)
    {
        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
            return NotFound(ApiResponseDto<int>.Error("Post nao encontrado."));

        post.ViewCount += 1;
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<int>.Success(post.ViewCount));
    }

    private static PostResponseDto MapToDto(Post p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Description = p.Description,
        Content = p.Content,
        Slug = p.Slug,
        Image = Helpers.GoogleDriveUrlHelper.CleanUrl(p.Image),
        Status = p.Status.ToString(),
        MetaTitle = p.MetaTitle,
        MetaDescription = p.MetaDescription,
        IsFeatured = p.IsFeatured,
        ReadingTime = p.ReadingTime,
        ViewCount = p.ViewCount,
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Author = p.Author != null ? new AuthorDto { Id = p.Author.Id, Name = p.Author.Name } : null,
        Tags = p.PostTags.Select(pt => new TagResponseDto { Id = pt.Tag.Id, Name = pt.Tag.Name, Slug = pt.Tag.Slug }).ToList()
    };
}
