FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/EventNest.Gateway/EventNest.Gateway.csproj services/EventNest.Gateway/
RUN dotnet restore services/EventNest.Gateway/EventNest.Gateway.csproj

COPY services/EventNest.Gateway/ services/EventNest.Gateway/
WORKDIR /src/services/EventNest.Gateway
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENV DOTNET_EnableDiagnostics=0
EXPOSE 5000
ENTRYPOINT ["dotnet", "EventNest.Gateway.dll"]
