# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Locatic.sln ./
COPY src/Locatic.Domain/Locatic.Domain.csproj src/Locatic.Domain/
COPY src/Locatic.Application/Locatic.Application.csproj src/Locatic.Application/
COPY src/Locatic.Infrastructure/Locatic.Infrastructure.csproj src/Locatic.Infrastructure/
COPY src/Locatic.Web/Locatic.Web.csproj src/Locatic.Web/

RUN dotnet restore src/Locatic.Web/Locatic.Web.csproj

COPY src/ src/
RUN dotnet publish src/Locatic.Web/Locatic.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# L'image aspnet:9.0 fournit déjà l'utilisateur non-root `app`.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /data \
    && chown -R app:app /data

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__LocaticDb="Data Source=/data/locatic.db" \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080
VOLUME ["/data"]

COPY --from=build --chown=app:app /app/publish .

USER app

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -fsS http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "Locatic.Web.dll"]
