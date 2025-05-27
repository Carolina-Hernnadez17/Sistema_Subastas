# Imagen base para ASP.NET Core Runtime (producción)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Imagen para compilar el proyecto (con SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo .csproj y restaurar dependencias
COPY Sistema_Subastas/*.csproj ./Sistema_Subastas/
WORKDIR /src/Sistema_Subastas
RUN dotnet restore

# Copiar todo el código y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Imagen final para producción
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Iniciar la aplicación
ENTRYPOINT ["dotnet", "Sistema_Subastas.dll"]
