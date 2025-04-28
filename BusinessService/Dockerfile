FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 6001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY BusinessService/BusinessService.csproj BusinessService/
RUN dotnet restore BusinessService/BusinessService.csproj

COPY BusinessService/ BusinessService/
WORKDIR /src/BusinessService
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development

RUN apt-get update && \
	apt-get install -y vim && \
	rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BusinessService.dll"]
