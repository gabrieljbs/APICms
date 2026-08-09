using Portfolio.Entities;
using Microsoft.EntityFrameworkCore;

namespace Portfolio;

public class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<Profile> Profiles => Set<Profile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity => entity.HasKey(p => p.Id));
        modelBuilder.Entity<Technology>(entity => entity.HasKey(t => t.Id));
        modelBuilder.Entity<SocialLink>(entity => entity.HasKey(s => s.Id));
        modelBuilder.Entity<Profile>(entity => entity.HasKey(p => p.Id));
    }
}
