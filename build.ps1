
$variants = "linux-x64", "linux-arm64", "win-x64", "win-arm64"
$projectName = "ASF-Bot.Telegram"
$frameworks = "net8.0", "net9.0"

$jobs = @();

foreach($framework in $frameworks){
foreach ($variant in $variants) {
    $buildArgs = '-r', "$variant", '-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true'
    $commonArgs = '-p:PublishTrimmed=true'

    if (($variant -notlike "win-*") -and ($winProjectNames -contains "$projectName")) {
        Write-Output "skip $projectName $variant"
        continue
    }
    else {
        $commonArgs = '-p:PublishTrimmed=false'
    }

    Write-Output "start build $projectName $variant"

    dotnet restore $projectName -p:ContinuousIntegrationBuild=true --nologo
    dotnet publish $projectName -c "Release" -f "$framework" -o "./dist/$variant/$projectName-$framework" --self-contained=true -p:ContinuousIntegrationBuild=true --no-restore --nologo $commonArgs $buildArgs

    # $jobs += Start-Job -ScriptBlock {
    #     param($variant)
    #     7z a -bd -slp -tzip -mm=Deflate -mx=5 -mfb=150 -mpass=10 "./dist/$variant-fde.zip" "./dist/$variant-fde/*"
    # } -ArgumentList $variant

    # $jobs += Start-Job -ScriptBlock {
    #     param($variant)
    #     7z a -bd -slp -tzip -mm=Deflate -mx=5 -mfb=150 -mpass=10 "./dist/$variant.zip" "./dist/$variant/*"
    # } -ArgumentList $variant
}
}

# 等待所有后台任务完成
foreach ($job in $jobs) {
    Wait-Job -Job $job
}

# 获取任务的输出
foreach ($job in $jobs) {
    Receive-Job -Job $job
}