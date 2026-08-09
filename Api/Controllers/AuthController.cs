using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog;
using Blog.Entities;
using Shared.DTOs.Auth;
using Shared.DTOs.Common;
using Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(BlogDbContext dbContext, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !user.Active || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return BadRequest(ApiResponseDto<AuthResponseDto>.Error("Credenciais invalidas ou usuario inativo."));
        }

        var token = GenerateJwtToken(user);
        var response = new AuthResponseDto
        {
            Token = token,
            User = new UserAuthDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Permission = user.Permission.ToString()
            }
        };

        return Ok(ApiResponseDto<AuthResponseDto>.Success(response, "Login realizado com sucesso."));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        if (await dbContext.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(ApiResponseDto<AuthResponseDto>.Error("E-mail ja cadastrado."));
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Permission = Role.User,
            Active = true
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var response = new AuthResponseDto
        {
            Token = token,
            User = new UserAuthDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Permission = user.Permission.ToString()
            }
        };

        return Ok(ApiResponseDto<AuthResponseDto>.Success(response, "Usuario registrado com sucesso."));
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Permission.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
