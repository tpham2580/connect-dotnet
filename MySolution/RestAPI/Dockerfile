FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RestAPI/RestAPI.csproj RestAPI/
RUN dotnet restore RestAPI/RestAPI.csproj

COPY RestAPI/ RestAPI/
WORKDIR /src/RestAPI
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development

RUN apt-get update && apt-get install -y \
	# Edit/view files
	vim \
	# Test HTTP endpoints
	curl \
	# Use ps, top, kill
	procps \
	# Tools like netstat, ifconfig
	net-tools \
	# Allows ping testing
	iputils-ping \
	# Use dig, nslookup for DNS lookup
	dnsutils \
	# 	Pager for viewing logs cleanly
	less \
	# List open files & ports
	lsof && \
	rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestAPI.dll"]
