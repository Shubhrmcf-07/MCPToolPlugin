FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /build

COPY MCPServer.sln .
COPY src/ src/

RUN dotnet restore
RUN dotnet publish src/MCPServer.Core -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=builder /app/publish .
RUN mkdir -p /app/plugins

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

ENTRYPOINT ["dotnet", "MCPServer.Core.dll"]
