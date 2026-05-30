using KumoBackup.Server.Domain.Entities;

namespace KumoBackup.Server.Domain.Contracts;

public sealed record BackupResponse(
    Guid Id,
    string FileName,
    long SizeBytes,
    string Sha256,
    string? DeviceAlias,
    string? AppVersion,
    DateTimeOffset CreatedAt)
{
    public static BackupResponse From(Backup backup) =>
        new(
            backup.Id,
            backup.FileName,
            backup.SizeBytes,
            backup.Sha256,
            backup.DeviceAlias,
            backup.AppVersion,
            backup.CreatedAt);
}
