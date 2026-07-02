FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/lancamentos-service/LancamentosService.csproj src/lancamentos-service/
RUN dotnet restore src/lancamentos-service/LancamentosService.csproj

COPY src/lancamentos-service src/lancamentos-service
RUN dotnet publish src/lancamentos-service/LancamentosService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LancamentosService.dll"]
