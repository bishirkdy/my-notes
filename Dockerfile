
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TechNotes.slnx ./
COPY src/TechNotes.Domain/TechNotes.Domain.csproj src/TechNotes.Domain/
COPY src/TechNotes.Application/TechNotes.Application.csproj src/TechNotes.Application/
COPY src/TechNotes.Infrastructure/TechNotes.Infrastructure.csproj src/TechNotes.Infrastructure/
COPY src/TechNotes.Persistence/TechNotes.Persistence.csproj src/TechNotes.Persistence/
COPY src/TechNotes.API/TechNotes.API.csproj src/TechNotes.API/
COPY tests/TechNotes.Application.Tests/TechNotes.Application.Tests.csproj tests/TechNotes.Application.Tests/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore TechNotes.slnx

COPY . .

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/TechNotes.API/TechNotes.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["/bin/sh", "-c", "if [ -n \"$ASPNETCORE_URLS\" ]; then exec dotnet TechNotes.API.dll --urls \"$ASPNETCORE_URLS\"; else exec dotnet TechNotes.API.dll --urls \"http://0.0.0.0:${PORT:-8080}\"; fi"]
