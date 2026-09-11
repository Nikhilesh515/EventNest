FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/EventNest.Shared/EventNest.Shared.Domain/EventNest.Shared.Domain.csproj services/EventNest.Shared/EventNest.Shared.Domain/
COPY services/EventNest.Shared/EventNest.Shared.Application/EventNest.Shared.Application.csproj services/EventNest.Shared/EventNest.Shared.Application/
COPY services/EventNest.Shared/EventNest.Shared.Infrastructure/EventNest.Shared.Infrastructure.csproj services/EventNest.Shared/EventNest.Shared.Infrastructure/
COPY services/EventNest.RSVPService/EventNest.RSVPService.Domain/EventNest.RSVPService.Domain.csproj services/EventNest.RSVPService/EventNest.RSVPService.Domain/
COPY services/EventNest.RSVPService/EventNest.RSVPService.Application/EventNest.RSVPService.Application.csproj services/EventNest.RSVPService/EventNest.RSVPService.Application/
COPY services/EventNest.RSVPService/EventNest.RSVPService.Infrastructure/EventNest.RSVPService.Infrastructure.csproj services/EventNest.RSVPService/EventNest.RSVPService.Infrastructure/
COPY services/EventNest.RSVPService/EventNest.RSVPService.API/EventNest.RSVPService.API.csproj services/EventNest.RSVPService/EventNest.RSVPService.API/
RUN dotnet restore services/EventNest.RSVPService/EventNest.RSVPService.API/EventNest.RSVPService.API.csproj

COPY services/EventNest.Shared/ services/EventNest.Shared/
COPY services/EventNest.RSVPService/ services/EventNest.RSVPService/
WORKDIR /src/services/EventNest.RSVPService/EventNest.RSVPService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5004
ENV DOTNET_EnableDiagnostics=0
EXPOSE 5004 51004
ENTRYPOINT ["dotnet", "EventNest.RSVPService.API.dll"]
