using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KumoBackup.Server.Domain.Contracts;

namespace KumoBackup.Test;

public sealed class ApiFlowTests(KumoBackupWebApplicationFactory factory)
    : IClassFixture<KumoBackupWebApplicationFactory>
{
    private static readonly string FixturePath = Path.Combine(
        AppContext.BaseDirectory,
        "TestFiles",
        "eu.kanade.tachiyomi_2025-01-19_21-54.tachibk");

    [Fact]
    public async Task BackupLifecycle_UploadsListsDownloadsAndDeletesBackup()
    {
        using var client = factory.CreateClient();

        var token = await CreateTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var serverInfo = await client.GetFromJsonAsync<ServerInfoResponse>("/api/server/info");
        Assert.NotNull(serverInfo);
        Assert.Equal("KumoBackup", serverInfo.Name);
        Assert.True(serverInfo.MaxUploadBytes > 0);

        await using var file = File.OpenRead(FixturePath);
        using var form = new MultipartFormDataContent
        {
            { new StreamContent(file), "file", Path.GetFileName(FixturePath) },
            { new StringContent("integration-test"), "deviceAlias" },
            { new StringContent("test-suite"), "appVersion" },
        };

        var uploadResponse = await client.PostAsync("/api/backups/upload", form);
        uploadResponse.EnsureSuccessStatusCode();
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<BackupResponse>();

        Assert.NotNull(uploaded);
        Assert.Equal(Path.GetFileName(FixturePath), uploaded.FileName);
        Assert.Equal(new FileInfo(FixturePath).Length, uploaded.SizeBytes);
        Assert.Equal("integration-test", uploaded.DeviceAlias);
        Assert.False(string.IsNullOrWhiteSpace(uploaded.Sha256));

        var backups = await client.GetFromJsonAsync<IReadOnlyList<BackupResponse>>("/api/backups");
        Assert.NotNull(backups);
        var listedBackup = Assert.Single(backups);
        Assert.Equal(uploaded.Id, listedBackup.Id);

        var downloadResponse = await client.GetAsync($"/api/backups/{uploaded.Id}/download");
        downloadResponse.EnsureSuccessStatusCode();
        var downloadedBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        var fixtureBytes = await File.ReadAllBytesAsync(FixturePath);
        Assert.Equal(fixtureBytes, downloadedBytes);

        var deleteResponse = await client.DeleteAsync($"/api/backups/{uploaded.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await client.GetFromJsonAsync<IReadOnlyList<BackupResponse>>("/api/backups");
        Assert.NotNull(afterDelete);
        Assert.Empty(afterDelete);
    }

    [Fact]
    public async Task BackupEndpoints_RejectMissingBearerToken()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/backups");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<CreateTokenResponse> CreateTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/setup/create-token",
            new CreateTokenRequest("integration-test"));
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<CreateTokenResponse>();
        Assert.NotNull(token);
        Assert.StartsWith("kb_live_", token.Token, StringComparison.Ordinal);
        return token;
    }
}
