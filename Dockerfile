# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY src/HydroGuard.API/HydroGuard.API.csproj src/HydroGuard.API/
RUN dotnet restore src/HydroGuard.API/HydroGuard.API.csproj
COPY src/HydroGuard.API/ src/HydroGuard.API/
RUN dotnet publish src/HydroGuard.API/HydroGuard.API.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build --chown=app:app /app/publish .
ARG APP_REVISION=local
ARG APP_SOURCE=
LABEL org.opencontainers.image.revision=$APP_REVISION org.opencontainers.image.source=$APP_SOURCE
ENV ASPNETCORE_URLS=http://+:8080 HydroGuard__Revision=$APP_REVISION
USER app
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=60s --retries=10 CMD curl -fsS http://localhost:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "HydroGuard.API.dll"]
