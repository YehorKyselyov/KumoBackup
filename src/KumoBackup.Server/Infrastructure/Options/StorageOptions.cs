namespace KumoBackup.Server.Infrastructure.Options;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Path { get; set; } = "/storage";
}
