using KumoBackup.Server.Infrastructure.Options;
using KumoBackup.Server.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KumoBackup.Test;

public sealed class KumoBackupWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string storagePath = Path.Combine(
        Path.GetTempPath(),
        "kumobackup-tests",
        Guid.NewGuid().ToString("N"));
    private readonly string databaseName = $"kumobackup-{Guid.NewGuid():N}";

    public string StoragePath => storagePath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var inMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseInMemoryDatabase(databaseName)
                    .UseInternalServiceProvider(inMemoryProvider));

            services.PostConfigure<StorageOptions>(options =>
            {
                options.Path = storagePath;
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (Directory.Exists(storagePath))
        {
            Directory.Delete(storagePath, recursive: true);
        }
    }
}
