# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ERAMonitor.API/ERAMonitor.API.csproj", "ERAMonitor.API/"]
COPY ["src/ERAMonitor.Core/ERAMonitor.Core.csproj", "ERAMonitor.Core/"]
COPY ["src/ERAMonitor.Infrastructure/ERAMonitor.Infrastructure.csproj", "ERAMonitor.Infrastructure/"]
COPY ["src/ERAMonitor.BackgroundJobs/ERAMonitor.BackgroundJobs.csproj", "ERAMonitor.BackgroundJobs/"]

RUN dotnet restore "ERAMonitor.API/ERAMonitor.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/ERAMonitor.API"
RUN dotnet build "ERAMonitor.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ERAMonitor.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ERAMonitor.API.dll"]
