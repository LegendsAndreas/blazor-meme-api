using BlazorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApi.Data;

public class AppDBContext : DbContext
{
    public DbSet<Meme> Memes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<MemesTags> MemesTags { get; set; }

    public AppDBContext(DbContextOptions<AppDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            // Configure foreign key to Role
            entity.HasOne(u => u.Roles)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configure MemesTags
        modelBuilder.Entity<MemesTags>()
            .HasOne(mt => mt.Meme)
            .WithMany(mt => mt.MemesTags)
            .HasForeignKey(mt => mt.MemeId);

        modelBuilder.Entity<MemesTags>()
            .HasOne(mt => mt.Tag)
            .WithMany(mt => mt.MemesTags)
            .HasForeignKey(mt => mt.TagId);
        
        SeedRoles(modelBuilder);
    }

    private void SeedRoles(ModelBuilder modelBuilder)
    {
        
        var roles = new[]
        {
            new Role
            {
                Id = "1",
                Name = "Guest",
                Description = "Standard user with basic permissions",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = "2",
                Name = "Admin",
                Description = "Administrator with full system access",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        };
        
        modelBuilder.Entity<Role>().HasData(roles);
    }
}