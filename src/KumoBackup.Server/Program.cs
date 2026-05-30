using KumoBackup.Server.Application;
using KumoBackup.Server.Infrastructure;
using KumoBackup.Server.Infrastructure.Persistence;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "KumoBackup API",
            Version = "v1",
            Description = "Self-hosted Mihon backup storage API.",
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "KumoBackup API token",
            In = ParameterLocation.Header,
            Description = "Enter a KumoBackup API token. Example: kb_live_xxxxx",
        });
});
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSwagger(options =>
{
    options.PreSerializeFilters.Add((document, request) =>
    {
        var forwardedPrefix = request.Headers["X-Forwarded-Prefix"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedPrefix))
        {
            document.Servers = [new OpenApiServer { Url = forwardedPrefix }];
        }
    });
});
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("v1/swagger.json", "KumoBackup API v1");
});
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();

public partial class Program;
