namespace KumoBackup.Server.Infrastructure.Options;

public sealed class BackupOptions
{
    public const string SectionName = "Backup";

    public long MaxUploadBytes { get; set; } = 104_857_600;
}
