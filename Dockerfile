# -------- Build Stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solution
COPY . .

# Restaura explicitamente o projeto
RUN dotnet restore "AlertConsumer/AlertConsumer.csproj"

# Publica
RUN dotnet publish "AlertConsumer/AlertConsumer.csproj" -c Release -o /app/publish

# -------- RUNTIME --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AlertConsumer.dll"]


