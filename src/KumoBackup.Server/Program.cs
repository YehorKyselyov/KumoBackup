using KumoBackup.Server.Application;
using KumoBackup.Server.Infrastructure;
using KumoBackup.Server.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Content("""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>KumoBackup</title>
  <style>
    body { font-family: system-ui, sans-serif; margin: 2rem; max-width: 48rem; line-height: 1.5; }
    code { background: #f2f4f8; padding: .15rem .35rem; border-radius: .25rem; }
  </style>
</head>
<body>
  <h1>KumoBackup</h1>
  <p>Server shell is running. Token management UI is the next web surface.</p>
  <p>Health: <code>/api/health</code></p>
</body>
</html>
""", "text/html"));

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
