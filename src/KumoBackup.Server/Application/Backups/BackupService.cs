using KumoBackup.Server.Application.Abstractions;
using KumoBackup.Server.Domain.Contracts;
using KumoBackup.Server.Domain.Entities;
using KumoBackup.Server.Infrastructure.Options;
using KumoBackup.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KumoBackup.Server.Application.Backups;

public sealed class BackupService(
    AppDbContext dbContext,
    IBackupStorage storage,
    IOptions<BackupOptions> backupOptions)
{
    public async Task<BackupUploadResult> UploadAsync(
        BackupUpload upload,
        CancellationToken cancellationToken)
    {
        if (upload.SizeBytes == 0)
        {
            return new BackupUploadResult.Invalid("Backup file is required.");
        }

        if (upload.SizeBytes > backupOptions.Value.MaxUploadBytes)
        {
            return new BackupUploadResult.TooLarge("Backup file is too large.");
        }

        var originalFileName = Path.GetFileName(upload.FileName);
        if (!originalFileName.EndsWith(".tachibk", StringComparison.OrdinalIgnoreCase))
        {
            return new BackupUploadResult.Invalid("Backup file must have a .tachibk extension.");
        }

        var backupId = Guid.NewGuid();
        var storedBackup = await storage.SaveAsync(upload.Content, backupId, cancellationToken);
        var backup = new Backup
        {
            Id = backupId,
            FileName = originalFileName,
            ContentType = string.IsNullOrWhiteSpace(upload.ContentType)
                ? "application/octet-stream"
                : upload.ContentType,
            SizeBytes = upload.SizeBytes,
            Sha256 = storedBackup.Sha256,
            StoragePath = storedBackup.StoragePath,
            DeviceAlias = NormalizeOptional(upload.DeviceAlias),
            AppVersion = NormalizeOptional(upload.AppVersion),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.Backups.Add(backup);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BackupUploadResult.Created(BackupResponse.From(backup));
    }

    public async Task<IReadOnlyList<BackupResponse>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Backups
            .OrderByDescending(backup => backup.CreatedAt)
            .Select(backup => BackupResponse.From(backup))
            .ToListAsync(cancellationToken);

    public async Task<BackupDownloadResult> GetDownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var backup = await dbContext.Backups.FindAsync([id], cancellationToken);
        if (backup is null)
        {
            return new BackupDownloadResult.NotFound();
        }

        var path = storage.GetReadablePath(backup.StoragePath);
        return File.Exists(path)
            ? new BackupDownloadResult.Found(path, backup.ContentType, backup.FileName)
            : new BackupDownloadResult.NotFound("Backup file is missing from storage.");
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var backup = await dbContext.Backups.FindAsync([id], cancellationToken);
        if (backup is null)
        {
            return false;
        }

        storage.DeleteIfExists(backup.StoragePath);
        dbContext.Backups.Remove(backup);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
