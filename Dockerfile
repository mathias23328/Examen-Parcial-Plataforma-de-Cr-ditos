# Usa la imagen oficial de .NET SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia los archivos del proyecto y restaura las dependencias
COPY *.csproj .
RUN dotnet restore

# Copia el resto del código y compila
COPY . .
RUN dotnet publish -c Release -o out

# Imagen final para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Variable de entorno para el puerto de Render
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
EXPOSE $PORT

# Comando para ejecutar la app
ENTRYPOINT ["dotnet", "Examen-Parcial-Plataforma-de-Cr-ditos.dll"]