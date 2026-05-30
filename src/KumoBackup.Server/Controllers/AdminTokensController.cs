using KumoBackup.Server.Application.Tokens;
using KumoBackup.Server.Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KumoBackup.Server.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/admin/tokens")]
public sealed class AdminTokensController(TokenService tokenService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TokenResponse>>> List(CancellationToken cancellationToken)
    {
        var tokens = await tokenService.ListAsync(cancellationToken);
        return Ok(tokens.Select(TokenResponse.From).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CreateTokenResponse>> Create(
        CreateTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Token name is required." });
        }

        var (token, rawToken) = await tokenService.CreateAsync(request.Name, cancellationToken);
        return Ok(CreateTokenResponse.From(token, rawToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        var revoked = await tokenService.RevokeAsync(id, cancellationToken);
        return revoked ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}/record")]
    public async Task<IActionResult> DeleteRevoked(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await tokenService.DeleteRevokedAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
