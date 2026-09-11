FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/EventNest.Shared/EventNest.Shared.Domain/EventNest.Shared.Domain.csproj services/EventNest.Shared/EventNest.Shared.Domain/
COPY services/EventNest.Shared/EventNest.Shared.Application/EventNest.Shared.Application.csproj services/EventNest.Shared/EventNest.Shared.Application/
COPY services/EventNest.Shared/EventNest.Shared.Infrastructure/EventNest.Shared.Infrastructure.csproj services/EventNest.Shared/EventNest.Shared.Infrastructure/
COPY services/EventNest.EventService/EventNest.EventService.Domain/EventNest.EventService.Domain.csproj services/EventNest.EventService/EventNest.EventService.Domain/
COPY services/EventNest.EventService/EventNest.EventService.Application/EventNest.EventService.Application.csproj services/EventNest.EventService/EventNest.EventService.Application/
COPY services/EventNest.EventService/EventNest.EventService.Infrastructure/EventNest.EventService.Infrastructure.csproj services/EventNest.EventService/EventNest.EventService.Infrastructure/
COPY services/EventNest.EventService/EventNest.EventService.API/EventNest.EventService.API.csproj services/EventNest.EventService/EventNest.EventService.API/
RUN dotnet restore services/EventNest.EventService/EventNest.EventService.API/EventNest.EventService.API.csproj

COPY services/EventNest.Shared/ services/EventNest.Shared/
COPY services/EventNest.EventService/ services/EventNest.EventService/
WORKDIR /src/services/EventNest.EventService/EventNest.EventService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5002
ENV DOTNET_EnableDiagnostics=0
EXPOSE 5002 51002
ENTRYPOINT ["dotnet", "EventNest.EventService.API.dll"]
