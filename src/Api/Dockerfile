FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MailForge.sln ./
COPY src/Domain/MailForge.Domain.csproj src/Domain/
COPY src/Shared/MailForge.Shared.csproj src/Shared/
COPY src/Application/MailForge.Application.csproj src/Application/
COPY src/Infrastructure/MailForge.Infrastructure.csproj src/Infrastructure/
COPY src/Api/MailForge.Api.csproj src/Api/
RUN dotnet restore src/Api/MailForge.Api.csproj
COPY src/ src/
RUN dotnet publish src/Api/MailForge.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /app/exports /app/uploads
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MailForge.Api.dll"]
