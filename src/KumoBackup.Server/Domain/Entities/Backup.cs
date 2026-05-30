namespace KumoBackup.Server.Domain.Entities;

public sealed class Backup
{
    public Guid Id { get; set; }

    public required string FileName { get; set; }

    public required string ContentType { get; set; }

    public long SizeBytes { get; set; }

    public required string Sha256 { get; set; }

    public required string StoragePath { get; set; }

    public string? DeviceAlias { get; set; }

    public string? AppVersion { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
