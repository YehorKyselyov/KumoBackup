namespace KumoBackup.Server.Application.Backups;

public abstract class BackupDownloadResult
{
    private BackupDownloadResult()
    {
    }

    public sealed class Found(
        string path,
        string contentType,
        string fileName) : BackupDownloadResult
    {
        public string Path { get; } = path;

        public string ContentType { get; } = contentType;

        public string FileName { get; } = fileName;
    }

    public sealed class NotFound(string? error = null) : BackupDownloadResult
    {
        public string? Error { get; } = error;
    }
}
