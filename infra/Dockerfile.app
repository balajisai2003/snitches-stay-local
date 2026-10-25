# ==============================================================================
# Dockerfile.app (Main ASP.NET Core Web API)
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution and csproj files for optimized Docker layer caching
COPY Organizer.slnx ./
COPY src/App/Organizer.App.csproj src/App/
COPY src/Processing/Organizer.Processing.csproj src/Processing/
COPY src/Modules/Activity/Organizer.Modules.Activity.csproj src/Modules/Activity/
COPY src/Modules/Ask/Organizer.Modules.Ask.csproj src/Modules/Ask/
COPY src/Modules/Documents/Organizer.Modules.Documents.csproj src/Modules/Documents/
COPY src/Modules/Identity/Organizer.Modules.Identity.csproj src/Modules/Identity/
COPY src/Modules/Search/Organizer.Modules.Search.csproj src/Modules/Search/
COPY src/Modules/Shared/Organizer.Modules.Shared.csproj src/Modules/Shared/

# Restore dependencies for App
RUN dotnet restore src/App/Organizer.App.csproj

# Copy all source files
COPY src/ src/

# Build and publish release binaries
WORKDIR /source/src/App
RUN dotnet publish Organizer.App.csproj -c Release -o /app/publish --no-restore -m:1

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Prepare shared volume directories
RUN mkdir -p /storage/originals /storage/results && chmod -R 777 /storage

ENV ASPNETCORE_URLS=http://+:8080
ENV Storage__RootPath=/storage
EXPOSE 8080

ENTRYPOINT ["dotnet", "Organizer.App.dll"]

