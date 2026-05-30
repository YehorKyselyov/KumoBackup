namespace KumoBackup.Server.Domain.Contracts;

public sealed record ServerInfoResponse(
    string Name,
    string Version,
    long MaxUploadBytes,
    DateTimeOffset ServerTime);
