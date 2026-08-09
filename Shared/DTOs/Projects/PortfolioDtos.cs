using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Projects;

public class CreateProjectDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? Image { get; set; }

    public string? Link { get; set; }
}

public class UpdateProjectDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Link { get; set; }
}

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Link { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTechnologyDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Logo { get; set; }
}

public class UpdateTechnologyDto
{
    public string? Name { get; set; }
    public string? Logo { get; set; }
}

public class TechnologyResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSocialLinkDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateSocialLinkDto
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public bool? IsActive { get; set; }
}

public class SocialLinkResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProfileDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }
}

public class UpdateProfileDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
}

public class ProfileResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
