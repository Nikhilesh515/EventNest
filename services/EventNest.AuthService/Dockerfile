FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/EventNest.Shared/EventNest.Shared.Domain/EventNest.Shared.Domain.csproj services/EventNest.Shared/EventNest.Shared.Domain/
COPY services/EventNest.Shared/EventNest.Shared.Application/EventNest.Shared.Application.csproj services/EventNest.Shared/EventNest.Shared.Application/
COPY services/EventNest.Shared/EventNest.Shared.Infrastructure/EventNest.Shared.Infrastructure.csproj services/EventNest.Shared/EventNest.Shared.Infrastructure/
COPY services/EventNest.AuthService/EventNest.AuthService.Domain/EventNest.AuthService.Domain.csproj services/EventNest.AuthService/EventNest.AuthService.Domain/
COPY services/EventNest.AuthService/EventNest.AuthService.Application/EventNest.AuthService.Application.csproj services/EventNest.AuthService/EventNest.AuthService.Application/
COPY services/EventNest.AuthService/EventNest.AuthService.Infrastructure/EventNest.AuthService.Infrastructure.csproj services/EventNest.AuthService/EventNest.AuthService.Infrastructure/
COPY services/EventNest.AuthService/EventNest.AuthService.API/EventNest.AuthService.API.csproj services/EventNest.AuthService/EventNest.AuthService.API/
RUN dotnet restore services/EventNest.AuthService/EventNest.AuthService.API/EventNest.AuthService.API.csproj

COPY services/EventNest.Shared/ services/EventNest.Shared/
COPY services/EventNest.AuthService/ services/EventNest.AuthService/
WORKDIR /src/services/EventNest.AuthService/EventNest.AuthService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5001
ENV DOTNET_EnableDiagnostics=0
EXPOSE 5001 51001
ENTRYPOINT ["dotnet", "EventNest.AuthService.API.dll"]
