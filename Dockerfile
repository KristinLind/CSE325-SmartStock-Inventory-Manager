FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SmartStock.Web/SmartStock.Web.csproj SmartStock.Web/
COPY Data/ Data/
COPY Models/ Models/
COPY Services/ Services/
RUN dotnet restore SmartStock.Web/SmartStock.Web.csproj

COPY . .
RUN dotnet publish SmartStock.Web/SmartStock.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SmartStock.Web.dll"]
