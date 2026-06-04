using Microsoft.EntityFrameworkCore;

namespace Xinglin.ReportEditor.Core.Data;

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
        // User seeding moved to TemplateSeedData.SeedUsers (runtime, config-driven).
        // Model-level seed (HasData) was removed because BCrypt hashing at migration
        // time is a side effect and hardcodes credentials.
    }
}
