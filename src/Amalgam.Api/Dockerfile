FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY src/Amalgam.Core/Amalgam.Core.csproj src/Amalgam.Core/
COPY src/Amalgam.Api/Amalgam.Api.csproj src/Amalgam.Api/
RUN dotnet restore src/Amalgam.Api/Amalgam.Api.csproj

# Copy source and publish
COPY src/Amalgam.Core/ src/Amalgam.Core/
COPY src/Amalgam.Api/ src/Amalgam.Api/
RUN dotnet publish src/Amalgam.Api/Amalgam.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Amalgam.Api.dll"]
