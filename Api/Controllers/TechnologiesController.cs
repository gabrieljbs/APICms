using Portfolio;
using Portfolio.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("technologies")]
public class TechnologiesController(PortfolioDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<TechnologyResponseDto>>>> GetAll()
    {
        var rawTech = await dbContext.Technologies
            .OrderBy(t => t.Name)
            .ToListAsync();

        var technologies = rawTech.Select(MapToDto).ToList();

        return Ok(ApiResponseDto<List<TechnologyResponseDto>>.Success(technologies));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TechnologyResponseDto>>> Create([FromBody] CreateTechnologyDto dto)
    {
        var tech = new Technology
        {
            Name = dto.Name,
            Logo = dto.Logo
        };

        dbContext.Technologies.Add(tech);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<TechnologyResponseDto>.Success(MapToDto(tech), "Tecnologia criada com sucesso."));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponseDto<TechnologyResponseDto>>> Update(Guid id, [FromBody] UpdateTechnologyDto dto)
    {
        var tech = await dbContext.Technologies.FindAsync(id);
        if (tech == null) return NotFound(ApiResponseDto<TechnologyResponseDto>.Error("Tecnologia nao encontrada."));

        if (dto.Name != null) tech.Name = dto.Name;
        if (dto.Logo != null) tech.Logo = dto.Logo;

        tech.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<TechnologyResponseDto>.Success(MapToDto(tech), "Tecnologia atualizada com sucesso."));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var tech = await dbContext.Technologies.FindAsync(id);
        if (tech == null) return NotFound(ApiResponseDto<bool>.Error("Tecnologia nao encontrada."));

        dbContext.Technologies.Remove(tech);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.Success(true, "Tecnologia removida com sucesso."));
    }

    private static TechnologyResponseDto MapToDto(Technology t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Logo = Helpers.GoogleDriveUrlHelper.CleanUrl(t.Logo),
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
