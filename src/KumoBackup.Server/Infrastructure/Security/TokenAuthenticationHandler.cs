using System.Security.Claims;
using System.Text.Encodings.Web;
using KumoBackup.Server.Application.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace KumoBackup.Server.Infrastructure.Security;

public sealed class TokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TokenService tokenService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaders))
        {
            return AuthenticateResult.NoResult();
        }

        var authorization = authorizationHeaders.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var rawToken = authorization["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return AuthenticateResult.Fail("Missing bearer token.");
        }

        var token = await tokenService.ValidateAsync(rawToken, Context.RequestAborted);
        if (token is null)
        {
            return AuthenticateResult.Fail("Invalid bearer token.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, token.Id.ToString()),
            new Claim(ClaimTypes.Name, token.Name),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
