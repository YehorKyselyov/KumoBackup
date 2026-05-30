using KumoBackup.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KumoBackup.Server.Application.Health;

public sealed class HealthService(AppDbContext dbContext)
{
    public Task<bool> CanConnectToDatabaseAsync(CancellationToken cancellationToken) =>
        dbContext.Database.CanConnectAsync(cancellationToken);
}
