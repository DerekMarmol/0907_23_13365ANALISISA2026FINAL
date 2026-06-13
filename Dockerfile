# 1. Etapa de compilación (SDK de .NET 8)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY EnviosRapidosGT/EnviosRapidosGT.csproj ./EnviosRapidosGT/
RUN dotnet restore ./EnviosRapidosGT/EnviosRapidosGT.csproj

COPY EnviosRapidosGT/ ./EnviosRapidosGT/
RUN dotnet publish ./EnviosRapidosGT/EnviosRapidosGT.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "EnviosRapidosGT.dll"]