#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
#RUN apt-get -y update
#RUN apt-get -y install git
RUN apk update && apk add --no-cache git
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GitOps.Updater.Cli/GitOps.Updater.Cli.csproj", "GitOps.Updater.Cli/"]
COPY ["Spectre.Console.Cli.Extensions/Spectre.Console.Cli.Extensions.csproj", "Spectre.Console.Cli.Extensions/"]
RUN dotnet restore "./GitOps.Updater.Cli/GitOps.Updater.Cli.csproj"
COPY . .
WORKDIR "/src/GitOps.Updater.Cli"
RUN dotnet build "./GitOps.Updater.Cli.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GitOps.Updater.Cli.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitOps.Updater.Cli.dll"]