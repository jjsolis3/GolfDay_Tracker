# ========================================
# GolfDay Tracker — Multi-stage Dockerfile
# ========================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY GolfDay_Tracker.sln ./
COPY src/GolfDay.Domain/GolfDay.Domain.csproj src/GolfDay.Domain/
COPY src/GolfDay.Application/GolfDay.Application.csproj src/GolfDay.Application/
COPY src/GolfDay.Infrastructure/GolfDay.Infrastructure.csproj src/GolfDay.Infrastructure/
COPY src/GolfDay.Web/GolfDay.Web.csproj src/GolfDay.Web/

# Restore dependencies
RUN dotnet restore "src/GolfDay.Web/GolfDay.Web.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR /src/src/GolfDay.Web
RUN dotnet build "GolfDay.Web.csproj" -c Release -o /app/build
RUN dotnet publish "GolfDay.Web.csproj" -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

# ----------------------------------------
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install tzdata for proper timezone support
RUN apt-get update && apt-get install -y --no-install-recommends tzdata && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN addgroup --system --gid 1001 golfday && \
    adduser --system --uid 1001 --ingroup golfday golfday

# Copy published output
COPY --from=build /app/publish .

# Create logs directory with proper permissions
RUN mkdir -p /app/logs && chown -R golfday:golfday /app

USER golfday

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GolfDay.Web.dll"]
