# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY UberEatsBackend/*.csproj UberEatsBackend/
RUN dotnet restore UberEatsBackend/UberEatsBackend.csproj

# Copy everything else and build
COPY UberEatsBackend/ UberEatsBackend/
WORKDIR /src/UberEatsBackend
RUN dotnet build UberEatsBackend.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish UberEatsBackend.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create directories
RUN mkdir -p wwwroot/uploads

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Expose port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "UberEatsBackend.dll"]