FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# The repository structure nests application code under a top-level `src` directory.
# After copying the repository into `/src`, project files live at `/src/src/...`.
# Adjust the publish path accordingly so `dotnet` can locate the project file.
RUN dotnet publish src/Services/Logger/Logger.Api/Logger.Api.csproj -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/logs"]
ENTRYPOINT ["dotnet", "Logger.Api.dll"]
