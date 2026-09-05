# Dockerfile para a API HydroGuard
# Usa imagem oficial do .NET 8 SDK para build e runtime para execução

# ===== ESTÁGIO 1: BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto e restaura dependências
COPY HydroGuard.API/HydroGuard.API.csproj HydroGuard.API/
RUN dotnet restore HydroGuard.API/HydroGuard.API.csproj

# Copia todo o código fonte
COPY HydroGuard.API/ HydroGuard.API/

# Compila a aplicação em modo Release
RUN dotnet build HydroGuard.API/HydroGuard.API.csproj -c Release -o /app/build

# Publica a aplicação (otimizada para produção)
RUN dotnet publish HydroGuard.API/HydroGuard.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== ESTÁGIO 2: RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia os arquivos publicados do estágio de build
COPY --from=build /app/publish .

# Expõe a porta 8080 (padrão ASP.NET Core em container)
EXPOSE 8080

# Define variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Inicia a aplicação
ENTRYPOINT ["dotnet", "HydroGuard.API.dll"]
