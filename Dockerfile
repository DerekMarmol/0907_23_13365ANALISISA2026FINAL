# 1. Etapa de compilación (SDK de .NET 8)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar los archivos de proyecto (.csproj) y restaurar dependencias
COPY EnviosRapidosGT/EnviosRapidosGT.csproj ./EnviosRapidosGT/
RUN dotnet restore ./EnviosRapidosGT/EnviosRapidosGT.csproj

# Copiar el resto del código del servidor y compilar en modo Release
COPY EnviosRapidosGT/ ./EnviosRapidosGT/
RUN dotnet publish ./EnviosRapidosGT/EnviosRapidosGT.csproj -c Release -o out

# 2. Etapa de ejecución (Runtime de .NET 8, mucho más liviano)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar los archivos compilados desde la etapa anterior
COPY --from=build-env /app/out .

# Configurar variables de entorno para que escuche en el puerto que Render le asigne
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "EnviosRapidosGT.dll"]