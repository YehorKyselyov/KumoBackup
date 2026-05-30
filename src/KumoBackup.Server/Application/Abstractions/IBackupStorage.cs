namespace KumoBackup.Server.Application.Abstractions;

public interface IBackupStorage
{
    Task<StoredBackup> SaveAsync(
        Stream input,
        Guid backupId,
        CancellationToken cancellationToken);

    string GetReadablePath(string storagePath);

    void DeleteIfExists(string storagePath);
}

public sealed class StoredBackup(string storagePath, string sha256)
{
    public string StoragePath { get; } = storagePath;

    public string Sha256 { get; } = sha256;
}
