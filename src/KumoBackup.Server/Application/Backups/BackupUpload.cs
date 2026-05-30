namespace KumoBackup.Server.Application.Backups;

public sealed class BackupUpload
{
    public required Stream Content { get; init; }

    public required string FileName { get; init; }

    public string? ContentType { get; init; }

    public long SizeBytes { get; init; }

    public string? DeviceAlias { get; init; }

    public string? AppVersion { get; init; }
}
