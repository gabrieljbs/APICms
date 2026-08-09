using System.Security.Claims;
using Blog;
using Blog.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("comments")]
public class CommentsController(BlogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<CommentResponseDto>>>> GetByPost([FromQuery] Guid postId)
    {
        var rawComments = await dbContext.Comments
            .Include(c => c.Author)
            .Include(c => c.Replies).ThenInclude(r => r.Author)
            .Where(c => c.PostId == postId && c.ParentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var comments = rawComments.Select(MapToDto).ToList();

        return Ok(ApiResponseDto<List<CommentResponseDto>>.Success(comments));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CommentResponseDto>>> Create([FromBody] CreateCommentDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var authorId))
            return Unauthorized();

        var comment = new Comment
        {
            Content = dto.Content,
            PostId = dto.PostId,
            AuthorId = authorId,
            ParentId = dto.ParentId
        };

        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();

        var created = await dbContext.Comments
            .Include(c => c.Author)
            .FirstAsync(c => c.Id == comment.Id);

        return Ok(ApiResponseDto<CommentResponseDto>.Success(MapToDto(created), "Comentario publicado."));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirstValue(ClaimTypes.Role);

        var comment = await dbContext.Comments.FindAsync(id);
        if (comment == null) return NotFound(ApiResponseDto<bool>.Error("Comentario nao encontrado."));

        if (comment.AuthorId.ToString() != userIdClaim && roleClaim != "Admin")
            return Forbid();

        dbContext.Comments.Remove(comment);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.Success(true, "Comentario removido."));
    }

    private static CommentResponseDto MapToDto(Comment c) => new()
    {
        Id = c.Id,
        Content = c.Content,
        PostId = c.PostId,
        AuthorId = c.AuthorId,
        AuthorName = c.Author?.Name ?? string.Empty,
        ParentId = c.ParentId,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Replies = c.Replies.Select(r => MapToDto(r)).ToList()
    };
}
