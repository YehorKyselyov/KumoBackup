namespace KumoBackup.Server.Domain.Entities;

public sealed class ApiToken
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string TokenHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
