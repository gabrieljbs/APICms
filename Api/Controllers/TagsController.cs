using Blog;
using Blog.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("tags")]
public class TagsController(BlogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<TagResponseDto>>>> GetAll()
    {
        var tags = await dbContext.Tags
            .Select(t => new TagResponseDto { Id = t.Id, Name = t.Name, Slug = t.Slug })
            .ToListAsync();

        return Ok(ApiResponseDto<List<TagResponseDto>>.Success(tags));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TagResponseDto>>> Create([FromBody] CreateTagDto dto)
    {
        var tag = new Tag { Name = dto.Name, Slug = dto.Slug };
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        var response = new TagResponseDto { Id = tag.Id, Name = tag.Name, Slug = tag.Slug };
        return Ok(ApiResponseDto<TagResponseDto>.Success(response, "Tag criada com sucesso."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var tag = await dbContext.Tags.FindAsync(id);
        if (tag == null) return NotFound(ApiResponseDto<bool>.Error("Tag nao encontrada."));

        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.Success(true, "Tag removida com sucesso."));
    }
}
