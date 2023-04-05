FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY /WebApi/WebApi.csproj /WebApi/
COPY /Infrastructure/Infrastructure.csproj /Infrastructure/
COPY /Domain/Domain.csproj /Domain/
RUN dotnet restore /WebApi/WebApi.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
COPY --from=build-env /app/out .
RUN apt-get update && apt-get install -y libgdiplus
ENTRYPOINT ["dotnet", "WebApi.dll"]
