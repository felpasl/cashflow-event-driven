FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/consolidado-service/ConsolidadoService.csproj src/consolidado-service/
RUN dotnet restore src/consolidado-service/ConsolidadoService.csproj

COPY src/consolidado-service src/consolidado-service
RUN dotnet publish src/consolidado-service/ConsolidadoService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ConsolidadoService.dll"]
