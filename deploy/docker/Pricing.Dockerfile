FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# Adjust for nested repository layout (`/src/src/...`).
RUN dotnet publish src/Services/Pricing/Pricing.Api/Pricing.Api.csproj -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Pricing.Api.dll"]
