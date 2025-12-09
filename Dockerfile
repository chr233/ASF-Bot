# Dockerfile for ASF-Bot.Telegram
# Multi-stage build with configurable .NET version via build-arg
ARG DOTNET_VERSION=10.0

ARG PROJECT_NAME="ASF-Bot.Telegram"
ARG TARGET_FRAMEWORK="net10.0"

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build

ARG PROJECT_NAME
ARG TARGETARCH
ARG TARGETOS
ARG TARGET_FRAMEWORK
ARG CONFIGURATION=Release
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DOTNET_NOLOGO=true

WORKDIR /src

# Copy solution and project files first to leverage Docker layer cache
COPY . .

RUN <<EOF
    set -eu

    case "$TARGETOS" in
        "linux") ;;
        *) echo "ERROR: Unsupported OS: ${TARGETOS}"; exit 1 ;;
    esac

    case "$TARGETARCH" in
        "amd64") framework="${TARGETOS}-x64" ;;
        "arm") framework="${TARGETOS}-${TARGETARCH}" ;;
        "arm64") framework="${TARGETOS}-${TARGETARCH}" ;;
        *) echo "ERROR: Unsupported CPU architecture: ${TARGETARCH}"; exit 1 ;;
    esac

    echo "Building for framework: $framework"

    echo "dotnet publish $PROJECT_NAME -c \"$CONFIGURATION\" -o \"/publish\" -p:UseAppHost=false -r \"$framework\" -f \"$TARGET_FRAMEWORK\" -p:ContinuousIntegrationBuild=true -p:PublishSingleFile=false -p:PublishTrimmed=false --nologo --no-self-contained"

    echo "Testing connection to nuget mirror..."
    if curl -s --connect-timeout 10 --max-time 30 https://repo.huaweicloud.com/repository/nuget/v3/index.json > /dev/null 2>&1; then
        echo "Test passed, nuget mirror is accessible"
        if [ -f "Nuget.config.bak" ]; then
            mv "Nuget.config.bak" "Nuget.config"
        else
            echo "WARNING: Nuget.config.bak not found"
        fi
    else
        echo "Test failed, use api.nuget.org instead"
    fi

    dotnet --info

    dotnet publish $PROJECT_NAME -c "$CONFIGURATION" -o "/publish" -r "$framework" -f "$TARGET_FRAMEWORK" -p:ContinuousIntegrationBuild=true -p:PublishSingleFile=false -p:PublishTrimmed=false -p:UseAppHost=false --nologo --no-self-contained

    mkdir /publish/config
    mv /publish/config.json /publish/config/config.json
EOF


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime

ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DOTNET_NOLOGO=true

WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /publish/ ./

ENV DOTNET_RUNNING_IN_CONTAINER=true

VOLUME ["/app/config", "/app/logs"]
HEALTHCHECK CMD ["pidof", "-q", "dotnet"]

ENTRYPOINT ["dotnet","./ASF-Bot.Telegram.dll"]