using KumoBackup.Server.Application.Tokens;
using KumoBackup.Server.Domain.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace KumoBackup.Server.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class SetupController(TokenService tokenService) : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<SetupStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var activeTokenCount = await tokenService.CountActiveAsync(cancellationToken);
        return Ok(new SetupStatusResponse(activeTokenCount > 0, activeTokenCount));
    }

    [HttpPost("create-token")]
    public async Task<ActionResult<CreateTokenResponse>> CreateToken(
        CreateTokenRequest request,
        CancellationToken cancellationToken)
    {
        var name = string.IsNullOrWhiteSpace(request.Name) ? "Initial token" : request.Name.Trim();
        var (token, rawToken) = await tokenService.CreateAsync(name, cancellationToken);

        return Ok(CreateTokenResponse.From(token, rawToken));
    }
}
