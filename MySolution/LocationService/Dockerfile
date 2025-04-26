FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 6000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY LocationService/LocationService.csproj LocationService/
RUN dotnet restore LocationService/LocationService.csproj

COPY LocationService/ LocationService/
WORKDIR /src/LocationService
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

RUN apt-get update && \
	apt-get install -y vim && \
	rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocationService.dll"]
