using Portfolio;
using Portfolio.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController(PortfolioDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<ProjectResponseDto>>>> GetAll()
    {
        var rawProjects = await dbContext.Projects
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var projects = rawProjects.Select(MapToDto).ToList();

        return Ok(ApiResponseDto<List<ProjectResponseDto>>.Success(projects));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProjectResponseDto>>> Create([FromBody] CreateProjectDto dto)
    {
        var project = new Project
        {
            Title = dto.Title,
            Description = dto.Description,
            Image = dto.Image,
            Link = dto.Link
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<ProjectResponseDto>.Success(MapToDto(project), "Projeto criado com sucesso."));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponseDto<ProjectResponseDto>>> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var project = await dbContext.Projects.FindAsync(id);
        if (project == null) return NotFound(ApiResponseDto<ProjectResponseDto>.Error("Projeto nao encontrado."));

        if (dto.Title != null) project.Title = dto.Title;
        if (dto.Description != null) project.Description = dto.Description;
        if (dto.Image != null) project.Image = dto.Image;
        if (dto.Link != null) project.Link = dto.Link;

        project.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<ProjectResponseDto>.Success(MapToDto(project), "Projeto atualizado com sucesso."));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var project = await dbContext.Projects.FindAsync(id);
        if (project == null) return NotFound(ApiResponseDto<bool>.Error("Projeto nao encontrado."));

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.Success(true, "Projeto removido com sucesso."));
    }

    private static ProjectResponseDto MapToDto(Project p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Description = p.Description,
        Image = Helpers.GoogleDriveUrlHelper.CleanUrl(p.Image),
        Link = p.Link,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
