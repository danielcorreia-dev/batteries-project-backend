FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env

WORKDIR /app

LABEL version="1.0" description="Faz deploy e cria uma imagem customizada baseada em duas imagens da Microsoft" org.opencontainers.image.authors="eugeniolopesfernandeslima1997@outlook.com"

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
ENTRYPOINT ["dotnet", "/app/WebApi.dll"]
