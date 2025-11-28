# Dockerfile for ASF-Bot.Telegram
# Multi-stage build with configurable .NET version via build-arg
ARG DOTNET_VERSION=10.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer cache
COPY . .

# Build for each runtime and framework
ARG PROJECT_NAME="ASF-Bot.Telegram"
ARG TARGET_RUNTIME 
ARG TARGET_FRAMEWORK

RUN echo "Building project: ${PROJECT_NAME} with .NET version: ${DOTNET_VERSION} ${TARGET_RUNTIME} ${TARGET_FRAMEWORK}" > 1.txt
RUN cat 1.txt

RUN dotnet publish ${PROJECT_NAME} -c Release -o /app/publish/${TARGET_RUNTIME}-${TARGET_FRAMEWORK} \
    -r ${TARGET_RUNTIME} -f ${TARGET_FRAMEWORK} --self-contained=true --nologo \
    -p:PublishTrimmed=false -p:PublishSingleFile=true -p:PublishReadyToRun=false \
    -p:IncludeNativeLibrariesForSelfExtract=true -p:ContinuousIntegrationBuild=true -p:UseAppHost=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS runtime
WORKDIR /app

# Copy the published files from the build stage
ARG TARGET_RUNTIME
ARG TARGET_FRAMEWORK
COPY --from=build /app/publish/${TARGET_RUNTIME}-${TARGET_FRAMEWORK} ./

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["./ASF-Bot.Telegram"]