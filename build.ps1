
$runtimes = "linux-x64", "linux-arm64", "win-x64", "win-arm64"
$framework = "net10.0"
$config = "Release"
$projectName = "ASF-Bot.Telegram"
$enableTrim = $true
$trimMode = "link"
$zip = $false

foreach ($runtime in $runtimes) {
    Write-Debug "Publishing for runtime: $runtime and framework: $framework"
    $buildName = "$projectName-$runtime-$framework"

    $outputDir = "./dist/$buildName"

    dotnet publish $projectName --output "$outputDir" --self-contained --runtime $runtime --framework $framework --configuration $config --no-restore --nologo -p:PublishTrimmed=$enableTrim -p:TrimMode=$trimMode -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:IncludeNativeLibrariesForSelfExtract=true -p:ContinuousIntegrationBuild=true -p:UseAppHost=true
    dotnet publish $projectName --output "$outputDir" --no-self-contained --runtime $runtime --framework $framework --configuration $config --no-restore --nologo -p:PublishTrimmed=$enableTrim -p:TrimMode=$trimMode -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:IncludeNativeLibrariesForSelfExtract=true -p:ContinuousIntegrationBuild=true -p:UseAppHost=true

    Remove-Item "$outputDir-fde/*.xml"
    Remove-Item "$outputDir/*.xml"

    if ($true -eq $zip) {
        Write-Debug "Creating zip archive for $buildName"
        7z a -bd -slp -tzip -mm=Deflate -mx=5 -mfb=150 -mpass=10 "./dist/$buildName-fde.zip" "./tmp/$buildName-fde/*"
        7z a -bd -slp -tzip -mm=Deflate -mx=5 -mfb=150 -mpass=10 "./dist/$buildName.zip" "./tmp/$buildName/*"
    }
}

if ($true -eq $zip) {
    Remove-Item -Recurse -Force "./tmp"
}