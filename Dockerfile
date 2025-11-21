# ============ BUILD STAGE ============
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо ВЕСЬ solution (FinTrack, WebApp, Tests...)
COPY . .

# Відновлюємо залежності лише для WebApp
RUN dotnet restore "./WebApp/WebApp.csproj"

# Публікуємо саме WebApp
RUN dotnet publish "./WebApp/WebApp.csproj" -c Release -o /app

# ============ RUNTIME STAGE ============
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "WebApp.dll"]
