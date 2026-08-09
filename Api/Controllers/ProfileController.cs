using Portfolio;
using Portfolio.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("profile")]
public class ProfileController(PortfolioDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<ProfileResponseDto>>> Get()
    {
        var profile = await dbContext.Profiles.FirstOrDefaultAsync();
        if (profile == null)
            return NotFound(ApiResponseDto<ProfileResponseDto>.Error("Perfil nao configurado."));

        return Ok(ApiResponseDto<ProfileResponseDto>.Success(MapToDto(profile)));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProfileResponseDto>>> Create([FromBody] CreateProfileDto dto)
    {
        var existing = await dbContext.Profiles.FirstOrDefaultAsync();
        if (existing != null)
            return BadRequest(ApiResponseDto<ProfileResponseDto>.Error("Ja existe um perfil cadastrado. Use PATCH para atualizar."));

        var profile = new Profile
        {
            Title = dto.Title,
            Description = dto.Description,
            PhotoUrl = dto.PhotoUrl
        };

        dbContext.Profiles.Add(profile);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<ProfileResponseDto>.Success(MapToDto(profile), "Perfil criado com sucesso."));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponseDto<ProfileResponseDto>>> Update(Guid id, [FromBody] UpdateProfileDto dto)
    {
        var profile = await dbContext.Profiles.FindAsync(id);
        if (profile == null) return NotFound(ApiResponseDto<ProfileResponseDto>.Error("Perfil nao encontrado."));

        if (dto.Title != null) profile.Title = dto.Title;
        if (dto.Description != null) profile.Description = dto.Description;
        if (dto.PhotoUrl != null) profile.PhotoUrl = dto.PhotoUrl;

        profile.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<ProfileResponseDto>.Success(MapToDto(profile), "Perfil atualizado com sucesso."));
    }

    private static ProfileResponseDto MapToDto(Profile p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Description = p.Description,
        PhotoUrl = Helpers.GoogleDriveUrlHelper.CleanUrl(p.PhotoUrl),
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
