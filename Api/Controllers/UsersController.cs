using Blog;
using Blog.Entities;
using Shared.DTOs.Common;
using Shared.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController(BlogDbContext dbContext) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PaginatedResponseDto<UserResponseDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        page = page < 1 ? 1 : page;
        limit = limit < 1 ? 10 : limit;

        var query = dbContext.Users;
        var total = await query.CountAsync();
        var rawUsers = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var users = rawUsers.Select(MapToDto).ToList();

        return Ok(new PaginatedResponseDto<UserResponseDto>
        {
            Data = users,
            Meta = new PaginationMeta
            {
                Page = page,
                Limit = limit,
                Total = total,
                LastPage = (int)Math.Ceiling(total / (double)limit)
            }
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> Create([FromBody] CreateUserDto dto)
    {
        if (await dbContext.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(ApiResponseDto<UserResponseDto>.Error("E-mail ja cadastrado."));

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Permission = dto.Permission,
            Active = true
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<UserResponseDto>.Success(MapToDto(user), "Usuario criado com sucesso."));
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user == null) return NotFound(ApiResponseDto<UserResponseDto>.Error("Usuario nao encontrado."));

        if (dto.Name != null) user.Name = dto.Name;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Password != null) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        if (dto.Permission.HasValue) user.Permission = dto.Permission.Value;
        if (dto.Active.HasValue) user.Active = dto.Active.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<UserResponseDto>.Success(MapToDto(user), "Usuario atualizado com sucesso."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user == null) return NotFound(ApiResponseDto<bool>.Error("Usuario nao encontrado."));

        user.DeletedAt = DateTime.UtcNow;
        user.Active = false;
        await dbContext.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.Success(true, "Usuario removido (soft delete)."));
    }

    private static UserResponseDto MapToDto(User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Permission = u.Permission.ToString(),
        Active = u.Active,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt,
        DeletedAt = u.DeletedAt
    };
}
