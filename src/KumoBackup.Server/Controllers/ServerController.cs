using System.Reflection;
using KumoBackup.Server.Domain.Contracts;
using KumoBackup.Server.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KumoBackup.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/server")]
public sealed class ServerController(IOptions<BackupOptions> backupOptions) : ControllerBase
{
    [HttpGet("info")]
    public ActionResult<ServerInfoResponse> Info()
    {
        var version = typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        return Ok(new ServerInfoResponse(
            "KumoBackup",
            string.IsNullOrWhiteSpace(version) ? "unknown" : version,
            backupOptions.Value.MaxUploadBytes,
            DateTimeOffset.UtcNow));
    }
}
