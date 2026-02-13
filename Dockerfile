# Use the official .NET 8.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Build argument for environment (default: Production)
ARG ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["UrlShortener.API/UrlShortener.API.csproj", "UrlShortener.API/"]
RUN dotnet restore "UrlShortener.API/UrlShortener.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/UrlShortener.API"
RUN dotnet build "UrlShortener.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UrlShortener.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables for the container
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "UrlShortener.API.dll"]