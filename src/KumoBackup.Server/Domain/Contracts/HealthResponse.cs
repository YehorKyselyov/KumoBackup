namespace KumoBackup.Server.Domain.Contracts;

public sealed record HealthResponse(
    string Status,
    string Database,
    DateTimeOffset CheckedAt);
