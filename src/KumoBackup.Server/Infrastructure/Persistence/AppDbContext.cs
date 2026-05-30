using KumoBackup.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KumoBackup.Server.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApiToken> ApiTokens => Set<ApiToken>();

    public DbSet<Backup> Backups => Set<Backup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiToken>(entity =>
        {
            entity.ToTable("api_tokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.Id).HasColumnName("id");
            entity.Property(token => token.Name).HasColumnName("name").IsRequired();
            entity.Property(token => token.TokenHash).HasColumnName("token_hash").IsRequired();
            entity.Property(token => token.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(token => token.LastUsedAt).HasColumnName("last_used_at");
            entity.Property(token => token.RevokedAt).HasColumnName("revoked_at");
            entity.HasIndex(token => token.TokenHash).IsUnique();
        });

        modelBuilder.Entity<Backup>(entity =>
        {
            entity.ToTable("backups");
            entity.HasKey(backup => backup.Id);
            entity.Property(backup => backup.Id).HasColumnName("id");
            entity.Property(backup => backup.FileName).HasColumnName("file_name").IsRequired();
            entity.Property(backup => backup.ContentType).HasColumnName("content_type").IsRequired();
            entity.Property(backup => backup.SizeBytes).HasColumnName("size_bytes").IsRequired();
            entity.Property(backup => backup.Sha256).HasColumnName("sha256").IsRequired();
            entity.Property(backup => backup.StoragePath).HasColumnName("storage_path").IsRequired();
            entity.Property(backup => backup.DeviceAlias).HasColumnName("device_alias");
            entity.Property(backup => backup.AppVersion).HasColumnName("app_version");
            entity.Property(backup => backup.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.HasIndex(backup => backup.CreatedAt);
        });
    }
}
