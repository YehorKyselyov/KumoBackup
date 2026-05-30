using System.Security.Cryptography;
using KumoBackup.Server.Domain.Entities;
using KumoBackup.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KumoBackup.Server.Application.Tokens;

public sealed class TokenService(AppDbContext dbContext)
{
    private const string TokenPrefix = "kb_live_";

    public async Task<(ApiToken Token, string RawToken)> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var rawToken = TokenPrefix + Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var token = new ApiToken
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            TokenHash = HashToken(rawToken),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.ApiTokens.Add(token);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (token, rawToken);
    }

    public async Task<IReadOnlyList<ApiToken>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.ApiTokens
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ApiToken?> ValidateAsync(string rawToken, CancellationToken cancellationToken)
    {
        if (!rawToken.StartsWith(TokenPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var tokenHash = HashToken(rawToken);
        var token = await dbContext.ApiTokens
            .SingleOrDefaultAsync(candidate =>
                candidate.TokenHash == tokenHash &&
                candidate.RevokedAt == null,
                cancellationToken);

        if (token is null)
        {
            return null;
        }

        token.LastUsedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<bool> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        var token = await dbContext.ApiTokens.FindAsync([id], cancellationToken);
        if (token is null)
        {
            return false;
        }

        if (token.RevokedAt is null)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> DeleteRevokedAsync(Guid id, CancellationToken cancellationToken)
    {
        var token = await dbContext.ApiTokens.FindAsync([id], cancellationToken);
        if (token?.RevokedAt is null)
        {
            return false;
        }

        dbContext.ApiTokens.Remove(token);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken) =>
        await dbContext.ApiTokens.CountAsync(token => token.RevokedAt == null, cancellationToken);

    private static string HashToken(string rawToken)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
