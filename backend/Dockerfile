# Multi-stage Dockerfile for CBS BI Platform .NET 8 API
# Build context: backend/

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY ["CBS.BI.Api/CBS.BI.Api/CBS.BI.Api.csproj", "CBS.BI.Api/CBS.BI.Api/"]
COPY ["CBS.BI.Api/CBS.BI.Application/CBS.BI.Application.csproj", "CBS.BI.Api/CBS.BI.Application/"]
COPY ["CBS.BI.Infrastructure/CBS.BI.Infrastructure.csproj", "CBS.BI.Infrastructure/"]
COPY ["CBS.BI.Domain/CBS.BI.Domain.csproj", "CBS.BI.Domain/"]

# Restore dependencies
RUN dotnet restore "CBS.BI.Api/CBS.BI.Api/CBS.BI.Api.csproj"

# Copy remaining source code
COPY . .

# Navigate to the API project directory for publish
WORKDIR "/src/CBS.BI.Api/CBS.BI.Api"

# Publish the application
RUN dotnet publish "CBS.BI.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Configure for Google Cloud Run
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Copy published application
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "CBS.BI.Api.dll"]
