# Dockerfile for ASF-Bot.Telegram
# Multi-stage build with configurable .NET version via build-arg
ARG DOTNET_VERSION=10.0
ARG PROJECT_NAME=ASF-Bot.Telegram

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer cache
COPY ASF-Bot.slnx ./
COPY ASF-Bot.Telegram/ASF-Bot.Telegram.csproj ASF-Bot.Telegram/
COPY ASF-Bot.Service/ASF-Bot.Service.csproj ASF-Bot.Service/
COPY ASF-Bot.Service.Telegram/ASF-Bot.Service.Telegram.csproj ASF-Bot.Service.Telegram/
COPY ASF-Bot.Infrastructure/ASF-Bot.Infrastructure.csproj ASF-Bot.Infrastructure/
COPY ASF-Bot.Data/ASF-Bot.Data.csproj ASF-Bot.Data/
COPY ASF-Bot.Model/ASF-Bot.Model.csproj ASF-Bot.Model/

# Restore dependencies
RUN dotnet restore ASF-Bot.slnx --disable-parallel -f net${DOTNET_VERSION}

# Copy the rest of the code and publish for Linux
COPY . .
WORKDIR /src
RUN dotnet publish ASF-Bot.Telegram -c Release -o /app/publish/linux -r linux-x64 --self-contained=true --nologo -p:PublishTrimmed=false -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:IncludeNativeLibrariesForSelfExtract=true -p:ContinuousIntegrationBuild=true -p:UseAppHost=true

# Publish for Windows
RUN dotnet publish ASF-Bot.Telegram -c Release -o /app/publish/windows -r win-x64 --self-contained=true --nologo -p:PublishTrimmed=false -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:IncludeNativeLibrariesForSelfExtract=true -p:ContinuousIntegrationBuild=true -p:UseAppHost=true

# Runtime stage for Linux
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS runtime-linux
WORKDIR /app
COPY --from=build /app/publish/linux ./
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["./ASF-Bot.Telegram"]

# Runtime stage for Windows
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS runtime-windows
WORKDIR /app
COPY --from=build /app/publish/windows ./
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["./ASF-Bot.Telegram"]