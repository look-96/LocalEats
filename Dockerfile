FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Restaurants.Domain/Restaurants.Domain.csproj src/Restaurants.Domain/
COPY src/Restaurants.Application/Restaurants.Application.csproj src/Restaurants.Application/
COPY src/Restaurants.Infrastructure/Restaurants.Infrastructure.csproj src/Restaurants.Infrastructure/
COPY src/Restaurants.PostgreSqlMigrations/Restaurants.PostgreSqlMigrations.csproj src/Restaurants.PostgreSqlMigrations/
COPY src/Restaurants.Maintenance/Restaurants.Maintenance.csproj src/Restaurants.Maintenance/
COPY src/Restaurants.API/Restaurants.API.csproj src/Restaurants.API/
COPY src/Restaurants.Client/Restaurants.Client.csproj src/Restaurants.Client/

RUN dotnet restore src/Restaurants.API/Restaurants.API.csproj
RUN dotnet restore src/Restaurants.Maintenance/Restaurants.Maintenance.csproj

COPY src/ src/

RUN dotnet publish src/Restaurants.API/Restaurants.API.csproj \
    --configuration Release --no-restore --output /out/api
RUN dotnet publish src/Restaurants.Maintenance/Restaurants.Maintenance.csproj \
    --configuration Release --no-restore --output /out/maintenance
RUN test -f /out/api/wwwroot/index.html \
    && find /out/api/wwwroot/_framework -maxdepth 1 -name 'icudt*.dat' -print -quit | grep -q . \
    && sed -i 's# https://localhost:7149 http://localhost:5125 ws://localhost:\* wss://localhost:\*##' /out/api/wwwroot/index.html

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /out/api/ ./
COPY --from=build /out/maintenance/ ./maintenance/
USER $APP_UID
ENTRYPOINT ["dotnet", "Restaurants.API.dll"]
