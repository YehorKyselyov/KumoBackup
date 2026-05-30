namespace KumoBackup.Server.Domain.Contracts;

public sealed record SetupStatusResponse(bool IsConfigured, int ActiveTokenCount);
