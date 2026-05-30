using KumoBackup.Server.Application.Health;
using KumoBackup.Server.Domain.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace KumoBackup.Server.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(HealthService healthService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        var databaseAvailable = await healthService.CanConnectToDatabaseAsync(cancellationToken);

        return Ok(new HealthResponse(
            Status: databaseAvailable ? "ok" : "degraded",
            Database: databaseAvailable ? "ok" : "unavailable",
            CheckedAt: DateTimeOffset.UtcNow));
    }
}
