# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TelegramBot.Tests/TelegramBot.Tests.csproj", "TelegramBot.Tests/"]
RUN dotnet restore "./TelegramBot.Tests/TelegramBot.Tests.csproj"
COPY . .
WORKDIR "/src/TelegramBot.Tests"
RUN dotnet build "./TelegramBot.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

ENTRYPOINT ["dotnet", "test"]