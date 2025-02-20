
$variants = "linux-x64", "linux-arm64", "win-x64", "win-arm64"
$projectName = "ASF-Bot.Telegram"

foreach ($variant in $variants) {
    $buildArgs = '-p:PublishSingleFile=true', '--self-contained=false'
    $buildFdeArgs = '-p:PublishSingleFile=true', '--self-contained=true'

    $commonArgs = '-r', "$variant", '-f', "net9.0", '-p:PublishTrimmed=true', '-p:IncludeNativeLibrariesForSelfExtract=true' , '-p:ContinuousIntegrationBuild=true', '--no-restore', '--nologo'

    Write-Output "start build $projectName $variant"

    dotnet restore $projectName -p:ContinuousIntegrationBuild=true --nologo
    dotnet publish $projectName -c "Release" -o "./dist/$projectName-$variant" $commonArgs $buildArgs
    dotnet publish $projectName -c "Release" -o "./dist/[fde]$projectName$variant" $commonArgs $buildFdeArgs

    Write-Output "build $projectName $variant complete"
}
