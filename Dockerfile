# Dockerfile for ASF-Bot.Telegram
# Multi-stage build with configurable .NET version via build-arg
ARG DOTNET_VERSION=10.0
ARG PROJECT_NAME="ASF-Bot.Telegram"
ARG TARGET_RUNTIME win-arm64
ARG TARGET_FRAMEWORK

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build

WORKDIR /src

SHELL ["pwsh", "-Command"]

# Copy solution and project files first to leverage Docker layer cache
COPY . .

# Build for each runtime and framework

RUN echo "Building project: ${PROJECT_NAME} with .NET version: ${DOTNET_VERSION} ${TARGET_RUNTIME} ${TARGET_FRAMEWORK}" > 1.txt && cat 1.txt

RUN dotnet nuget list source

RUN dotnet restore ${PROJECT_NAME} --disable-parallel --interactive 


RUN <<EOF



EOF

RUN dotnet publish ${PROJECT_NAME} -c Release -o /publish/ \
    -r ${TARGET_RUNTIME} -f ${TARGET_FRAMEWORK} --self-contained=true --nologo \
    -p:PublishTrimmed=false -p:PublishSingleFile=true -p:PublishReadyToRun=false \
    -p:IncludeNativeLibrariesForSelfExtract=true -p:ContinuousIntegrationBuild=true -p:UseAppHost=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime

ARG PROJECT_NAME
ARG TARGET_RUNTIME win-arm64
ARG TARGET_FRAMEWORK


RUN echo "Building project: ${PROJECT_NAME} with .NET version: ${DOTNET_VERSION} ${TARGET_RUNTIME} ${TARGET_FRAMEWORK}" TZ ${TZ} > 1.txt && cat 1.txt

WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /publish/* ./

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["./ASF-Bot.Telegram"]