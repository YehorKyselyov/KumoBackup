using KumoBackup.Server.Domain.Entities;

namespace KumoBackup.Server.Domain.Contracts;

public sealed record CreateTokenResponse(
    Guid Id,
    string Name,
    string Token)
{
    public static CreateTokenResponse From(ApiToken token, string rawToken) =>
        new(token.Id, token.Name, rawToken);
}
