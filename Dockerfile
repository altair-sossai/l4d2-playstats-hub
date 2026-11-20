FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /source
COPY src/L4D2PlayStats.Hub.Core/L4D2PlayStats.Hub.Core.csproj src/L4D2PlayStats.Hub.Core/
COPY src/L4D2PlayStats.Hub.Migrations.MySql/L4D2PlayStats.Hub.Migrations.MySql.csproj src/L4D2PlayStats.Hub.Migrations.MySql/
COPY src/L4D2PlayStats.Hub.Migrations.PostgreSql/L4D2PlayStats.Hub.Migrations.PostgreSql.csproj src/L4D2PlayStats.Hub.Migrations.PostgreSql/
COPY src/L4D2PlayStats.Hub.Migrations.SqlServer/L4D2PlayStats.Hub.Migrations.SqlServer.csproj src/L4D2PlayStats.Hub.Migrations.SqlServer/
COPY src/L4D2PlayStats.Hub.Migrations.Sqlite/L4D2PlayStats.Hub.Migrations.Sqlite.csproj src/L4D2PlayStats.Hub.Migrations.Sqlite/
COPY src/L4D2PlayStats.Hub.Web/L4D2PlayStats.Hub.Web.csproj src/L4D2PlayStats.Hub.Web/
RUN dotnet restore src/L4D2PlayStats.Hub.Web/L4D2PlayStats.Hub.Web.csproj
COPY src/ src/
RUN dotnet publish src/L4D2PlayStats.Hub.Web --configuration Release --no-restore --output /app -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app .
COPY --chmod=755 deploy/docker/entrypoint.sh /usr/local/bin/hub-entrypoint
ENV ASPNETCORE_URLS=http://+:8080 \
    Hub__DataDirectory=/data \
    Hub__SecretsKeyFile=/secrets/secrets.key
VOLUME ["/data", "/secrets"]
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 CMD ["dotnet", "/app/L4D2PlayStats.Hub.Web.dll", "health"]
ENTRYPOINT ["hub-entrypoint"]
