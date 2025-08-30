FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# The source code resides under the top-level `src` directory.
# After copying the repository into `/src`, adjust the project path accordingly.
RUN dotnet publish src/Services/Data/Data.Api/Data.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/data"]
ENTRYPOINT ["dotnet", "Data.Api.dll"]
