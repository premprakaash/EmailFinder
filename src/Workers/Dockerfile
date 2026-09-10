FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MailForge.sln ./
COPY src/Domain/MailForge.Domain.csproj src/Domain/
COPY src/Shared/MailForge.Shared.csproj src/Shared/
COPY src/Application/MailForge.Application.csproj src/Application/
COPY src/Infrastructure/MailForge.Infrastructure.csproj src/Infrastructure/
COPY src/Workers/MailForge.Workers.csproj src/Workers/
RUN dotnet restore src/Workers/MailForge.Workers.csproj
COPY src/ src/
RUN dotnet publish src/Workers/MailForge.Workers.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /app/exports /app/uploads
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MailForge.Workers.dll"]
