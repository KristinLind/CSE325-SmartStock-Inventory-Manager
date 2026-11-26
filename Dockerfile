FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish SmartStock.Web/SmartStock.Web.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

RUN test -s /app/publish/wwwroot/_framework/blazor.web.js

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SmartStock.Web.dll"]