using System.Net;
using System.Net.Http.Json;
using KumoBackup.Server.Domain.Contracts;

namespace KumoBackup.Test;

public sealed class AdminTokenTests(KumoBackupWebApplicationFactory factory)
    : IClassFixture<KumoBackupWebApplicationFactory>
{
    [Fact]
    public async Task AdminTokenApi_CreatesListsAndRevokesTokens()
    {
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/admin/tokens",
            new CreateTokenRequest("web-admin"));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CreateTokenResponse>();

        Assert.NotNull(created);
        Assert.StartsWith("kb_live_", created.Token, StringComparison.Ordinal);

        var tokens = await client.GetFromJsonAsync<IReadOnlyList<TokenResponse>>("/api/admin/tokens");
        Assert.NotNull(tokens);
        var token = Assert.Single(tokens);
        Assert.Equal(created.Id, token.Id);
        Assert.Equal("web-admin", token.Name);
        Assert.Null(token.RevokedAt);

        var revokeResponse = await client.DeleteAsync($"/api/admin/tokens/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        tokens = await client.GetFromJsonAsync<IReadOnlyList<TokenResponse>>("/api/admin/tokens");
        Assert.NotNull(tokens);
        token = Assert.Single(tokens);
        Assert.NotNull(token.RevokedAt);

        var deleteResponse = await client.DeleteAsync($"/api/admin/tokens/{created.Id}/record");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        tokens = await client.GetFromJsonAsync<IReadOnlyList<TokenResponse>>("/api/admin/tokens");
        Assert.NotNull(tokens);
        Assert.Empty(tokens);
    }

    [Fact]
    public async Task LandingPage_ReturnsTokenAdministrationUi()
    {
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("Token administration", html, StringComparison.Ordinal);
        Assert.Contains("favicon.ico", html, StringComparison.Ordinal);
        Assert.Contains("js/admin.js", html, StringComparison.Ordinal);

        var favicon = await client.GetAsync("/favicon.ico");
        favicon.EnsureSuccessStatusCode();

        var script = await client.GetStringAsync("/js/admin.js");
        Assert.Contains("api/admin/tokens", script, StringComparison.Ordinal);
    }
}
