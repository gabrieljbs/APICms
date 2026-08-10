using System.Text;
using Blog;
using Blog.Entities;
using Portfolio;
using Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- DbContexts ---
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BlogDb")));

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PortfolioDb")));

// --- JWT Auth ---
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key nao configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000", "http://localhost:3001", "http://localhost:4200"]
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- Seed Initial Admin User if table is empty (apenas em ambiente de desenvolvimento) ---
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var blogDb = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
            if (!blogDb.Users.Any())
            {
                var seedEmail = builder.Configuration["Seed:AdminEmail"] ?? "admin@devsuite.com";
                var seedPassword = builder.Configuration["Seed:AdminPassword"] ?? "admin123";
                var adminUser = new User
                {
                    Name = "Administrador Root",
                    Email = seedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedPassword),
                    Permission = Role.Admin,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                blogDb.Users.Add(adminUser);
                blogDb.SaveChanges();
                Console.WriteLine($"[Seeder]: Usuario Admin inicial criado para desenvolvimento ({seedEmail})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seeder Error]: {ex.Message}");
        }
    }
}

app.UseCors();

// Habilita Swagger em todos os ambientes (inclusive producao) e define na raiz do site
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1");
    c.RoutePrefix = string.Empty; // Abre o Swagger diretamente na raiz (https://seu-site.monsterasp.net/)
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Status = "Online", Message = "Api DevSuite operando com sucesso", Timestamp = DateTime.UtcNow }));

app.Run();
