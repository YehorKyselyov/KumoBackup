using KumoBackup.Server.Domain.Contracts;

namespace KumoBackup.Server.Application.Backups;

public abstract class BackupUploadResult
{
    private BackupUploadResult()
    {
    }

    public sealed class Created(BackupResponse backup) : BackupUploadResult
    {
        public BackupResponse Backup { get; } = backup;
    }

    public sealed class Invalid(string error) : BackupUploadResult
    {
        public string Error { get; } = error;
    }

    public sealed class TooLarge(string error) : BackupUploadResult
    {
        public string Error { get; } = error;
    }
}
