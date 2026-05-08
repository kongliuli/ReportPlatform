using Microsoft.EntityFrameworkCore;

namespace Xinglin.WebReportEditor.Core.Data;

public class TemplateDbContext : DbContext
{
    public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
    {
    }

    public DbSet<TemplateEntity> Templates => Set<TemplateEntity>();
    public DbSet<TemplateVersionEntity> TemplateVersions => Set<TemplateVersionEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TemplateEntity>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.HospitalId);
            entity.HasIndex(e => new { e.Type, e.HospitalId });

            entity.Property(e => e.ContentJson)
                .HasColumnType("TEXT");

            entity.HasMany(e => e.Versions)
                .WithOne(v => v.Template)
                .HasForeignKey(v => v.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateVersionEntity>(entity =>
        {
            entity.HasIndex(e => e.TemplateId);
            entity.HasIndex(e => new { e.TemplateId, e.VersionNumber }).IsUnique();

            entity.Property(e => e.ContentJson)
                .HasColumnType("TEXT");
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasMany(e => e.RefreshTokens)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshTokenEntity>(entity =>
        {
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UserId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        modelBuilder.Entity<UserEntity>().HasData(
            new UserEntity
            {
                Id = adminId,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                DisplayName = "系统管理员",
                Role = "admin",
                HospitalId = "H001",
                IsActive = true,
                CreateTime = DateTime.UtcNow
            },
            new UserEntity
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Username = "editor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("editor123"),
                DisplayName = "编辑员",
                Role = "editor",
                HospitalId = "H001",
                IsActive = true,
                CreateTime = DateTime.UtcNow
            }
        );
    }
}
