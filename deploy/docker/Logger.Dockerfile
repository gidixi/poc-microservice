FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

RUN dotnet publish /src/Services/Logger/Logger.Api/Logger.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/logs"]
ENTRYPOINT ["dotnet", "Logger.Api.dll"]
