# Dockerfile for ASF-Bot.Telegram
# Multi-stage build with configurable .NET version via build-arg
ARG DOTNET_VERSION=10.0

ARG PROJECT_NAME="ASF-Bot.Telegram"
ARG TARGET_RUNTIME=linux-amd64
ARG TARGET_FRAMEWORK=net10.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build

ARG PROJECT_NAME
ARG TARGET_RUNTIME
ARG TARGET_FRAMEWORK

WORKDIR /src

# Copy solution and project files first to leverage Docker layer cache
COPY . .

SHELL [ "pwsh", "-Command" ]

RUN <<EOF
    $projectName=${env:PROJECT_NAME}
    $framework=${env:TARGET_FRAMEWORK}
    $runtime=${env:TARGET_RUNTIME}

    Write-Host "Building project: $projectName with .NET version: $TARGET_RUNTIME $framework" 

    dotnet publish $projectName --configuration Release --output /publish --runtime $runtime --framework $framework --no-self-contained -p:PublishTrimmed=false -p:ContinuousIntegrationBuild=true -p:UseAppHost=false -p:PublishSingleFile=false --nologo

    New-Item -ItemType Directory -Path "/publish/config"
    Move-Item -Path "/publish/config.json" -Destination "/publish/config/config.json"
    
    Write-Host "Contents of /publish directory:"
    Get-ChildItem -Path "/publish" -Recurse | ForEach-Object { Write-Host $_.FullName }
    
EOF


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime

WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /publish/ ./

ENV DOTNET_RUNNING_IN_CONTAINER=true

VOLUME ["/app/config", "/app/logs"]
HEALTHCHECK CMD ["pidof", "-q", "dotnet"]

ENTRYPOINT ["dotnet","./ASF-Bot.Telegram.dll"]