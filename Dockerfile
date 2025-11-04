FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar apenas o projeto da API para restaurar dependências
COPY src/UrlShortener.Api/*.csproj ./src/UrlShortener.Api/
WORKDIR /app/src/UrlShortener.Api
RUN dotnet restore

# Copiar código da API e buildar
COPY src/UrlShortener.Api/ ./
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
