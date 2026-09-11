FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/EventNest.Shared/EventNest.Shared.Domain/EventNest.Shared.Domain.csproj services/EventNest.Shared/EventNest.Shared.Domain/
COPY services/EventNest.Shared/EventNest.Shared.Application/EventNest.Shared.Application.csproj services/EventNest.Shared/EventNest.Shared.Application/
COPY services/EventNest.Shared/EventNest.Shared.Infrastructure/EventNest.Shared.Infrastructure.csproj services/EventNest.Shared/EventNest.Shared.Infrastructure/
COPY services/EventNest.TagService/EventNest.TagService.Domain/EventNest.TagService.Domain.csproj services/EventNest.TagService/EventNest.TagService.Domain/
COPY services/EventNest.TagService/EventNest.TagService.Application/EventNest.TagService.Application.csproj services/EventNest.TagService/EventNest.TagService.Application/
COPY services/EventNest.TagService/EventNest.TagService.Infrastructure/EventNest.TagService.Infrastructure.csproj services/EventNest.TagService/EventNest.TagService.Infrastructure/
COPY services/EventNest.TagService/EventNest.TagService.API/EventNest.TagService.API.csproj services/EventNest.TagService/EventNest.TagService.API/
RUN dotnet restore services/EventNest.TagService/EventNest.TagService.API/EventNest.TagService.API.csproj

COPY services/EventNest.Shared/ services/EventNest.Shared/
COPY services/EventNest.TagService/ services/EventNest.TagService/
WORKDIR /src/services/EventNest.TagService/EventNest.TagService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5003
ENV DOTNET_EnableDiagnostics=0
EXPOSE 5003 51003
ENTRYPOINT ["dotnet", "EventNest.TagService.API.dll"]
