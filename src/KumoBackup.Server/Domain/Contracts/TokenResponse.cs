using KumoBackup.Server.Domain.Entities;

namespace KumoBackup.Server.Domain.Contracts;

public sealed record TokenResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt,
    DateTimeOffset? RevokedAt)
{
    public static TokenResponse From(ApiToken token) =>
        new(
            token.Id,
            token.Name,
            token.CreatedAt,
            token.LastUsedAt,
            token.RevokedAt);
}
