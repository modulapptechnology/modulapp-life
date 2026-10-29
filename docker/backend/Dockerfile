# Build context: the repository root (see docker/docker-compose-*.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY life-backend/ ./
# The tracked files are Life.Api*.csproj but the solution and the references say Life.API*: Windows ignores
# the case, Linux does not.
RUN mv API/Life.Api.csproj API/Life.API.csproj \
    && mv tests/API.Tests/Life.Api.Tests.csproj tests/API.Tests/Life.API.Tests.csproj
RUN dotnet restore life-backend.slnx
RUN dotnet build life-backend.slnx --configuration Release --no-restore

FROM build AS test
# Unit tests only: the integration and critical-flow tests need the test database server.
CMD ["sh", "-c", "dotnet test tests/Domain.Tests/Life.Domain.Tests.csproj --configuration Release --no-build && dotnet test tests/Application.Tests/Life.Application.Tests.csproj --configuration Release --no-build"]

FROM build AS migrations
# dotnet-ef comes from the tool manifest of the repository (.config/dotnet-tools.json).
RUN dotnet tool restore
# Applies the pending migrations to the database set in ConnectionStrings__Default and exits.
CMD ["dotnet", "ef", "database", "update", "--project", "Persistence/Life.Persistence.csproj", "--startup-project", "API/Life.API.csproj", "--configuration", "Release", "--no-build"]

FROM build AS publish
RUN dotnet publish API/Life.API.csproj --configuration Release --no-build --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app ./
ENV ASPNETCORE_URLS=http://+:5187
EXPOSE 5187
ENTRYPOINT ["dotnet", "Life.API.dll"]
