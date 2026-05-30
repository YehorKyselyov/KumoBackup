using KumoBackup.Server.Application.Backups;
using KumoBackup.Server.Application.Health;
using KumoBackup.Server.Application.Tokens;

namespace KumoBackup.Server.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<BackupService>();
        services.AddScoped<HealthService>();
        services.AddScoped<TokenService>();

        return services;
    }
}
