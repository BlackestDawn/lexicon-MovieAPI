# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy just the project files first so `dotnet restore` is cached independently
# of source-code changes.
COPY backend/src/MovieAPI.Api/MovieAPI.Api.csproj src/MovieAPI.Api/
COPY backend/src/MovieAPI.Application/MovieAPI.Application.csproj src/MovieAPI.Application/
COPY backend/src/MovieAPI.Domain/MovieAPI.Domain.csproj src/MovieAPI.Domain/
COPY backend/src/MovieAPI.Infrastructure/MovieAPI.Infrastructure.csproj src/MovieAPI.Infrastructure/
RUN dotnet restore src/MovieAPI.Api/MovieAPI.Api.csproj

COPY src/ src/
RUN dotnet publish src/MovieAPI.Api/MovieAPI.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

# External services are wired up entirely through configuration, not baked into
# the image - override these at `docker run`/compose time via ASP.NET Core's
# double-underscore env var convention:
#   ConnectionStrings__sqlserver   SQL Server connection string (required)
#   ConnectionStrings__redis       Redis connection string, backs the output cache (required)
#   Elasticsearch__Uri             Elasticsearch endpoint for Serilog (required)
#   Jwt__Issuer / Jwt__Audience    JWT issuer/audience (required)
#   Jwt__Key                       JWT signing key (required, keep secret)
#   Jwt__ExpiryMinutes             Access token lifetime (optional, defaults to appsettings.json)
#   Seed__AdminEmail / Seed__AdminPassword   Bootstrap admin account (optional, opt-in)
#   ApplyMigrationsOnStartup       "true" to run EF Core migrations at boot (optional, off by default)

COPY --from=build /app/publish .

# The base image ships a non-root "app" user (UID 64198) for exactly this purpose.
USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "MovieAPI.Api.dll"]
