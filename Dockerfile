FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/KumoBackup.Server/KumoBackup.Server.csproj src/KumoBackup.Server/
RUN dotnet restore src/KumoBackup.Server/KumoBackup.Server.csproj

COPY src/KumoBackup.Server src/KumoBackup.Server
RUN dotnet publish src/KumoBackup.Server/KumoBackup.Server.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KumoBackup.Server.dll"]
