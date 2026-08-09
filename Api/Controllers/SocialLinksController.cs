using Portfolio;
using Portfolio.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("social-links")]
public class SocialLinksController(PortfolioDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<SocialLinkResponseDto>>>> GetAll()
    {
        var rawLinks = await dbContext.SocialLinks
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var links = rawLinks.Select(MapToDto).ToList();

        return Ok(ApiResponseDto<List<SocialLinkResponseDto>>.Success(links));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<SocialLinkResponseDto>>> Create([FromBody] CreateSocialLinkDto dto)
    {
        var link = new SocialLink
        {
            Name = dto.Name,
            Url = dto.Url,
            Icon = dto.Icon,
            IsActive = dto.IsActive
        };

        dbContext.SocialLinks.Add(link);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<SocialLinkResponseDto>.Success(MapToDto(link), "Link social criado com sucesso."));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponseDto<SocialLinkResponseDto>>> Update(Guid id, [FromBody] UpdateSocialLinkDto dto)
    {
        var link = await dbContext.SocialLinks.FindAsync(id);
        if (link == null) return NotFound(ApiResponseDto<SocialLinkResponseDto>.Error("Link social nao encontrado."));

        if (dto.Name != null) link.Name = dto.Name;
        if (dto.Url != null) link.Url = dto.Url;
        if (dto.Icon != null) link.Icon = dto.Icon;
        if (dto.IsActive.HasValue) link.IsActive = dto.IsActive.Value;

        link.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<SocialLinkResponseDto>.Success(MapToDto(link), "Link social atualizado com sucesso."));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var link = await dbContext.SocialLinks.FindAsync(id);
        if (link == null) return NotFound(ApiResponseDto<bool>.Error("Link social nao encontrado."));

        dbContext.SocialLinks.Remove(link);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.Success(true, "Link social removido com sucesso."));
    }

    private static SocialLinkResponseDto MapToDto(SocialLink s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Url = s.Url,
        Icon = s.Icon,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
