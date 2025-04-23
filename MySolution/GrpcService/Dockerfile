FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 6000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY GrpcService/GrpcService.csproj GrpcService/
RUN dotnet restore GrpcService/GrpcService.csproj

COPY GrpcService/ GrpcService/
WORKDIR /src/GrpcService
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

RUN apt-get update && \
	apt-get install -y vim && \
	rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GrpcService.dll"]
