# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY InstantWellness.slnx ./
COPY src/InstantWellness.Api/InstantWellness.Api.csproj src/InstantWellness.Api/
COPY src/InstantWellness.Application/InstantWellness.Application.csproj src/InstantWellness.Application/
COPY src/InstantWellness.Domain/InstantWellness.Domain.csproj src/InstantWellness.Domain/
COPY src/InstantWellness.Infrastructure/InstantWellness.Infrastructure.csproj src/InstantWellness.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/InstantWellness.Api/InstantWellness.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render uses PORT env variable
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "InstantWellness.Api.dll"]