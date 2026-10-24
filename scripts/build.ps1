param([switch]$OfflineInstalledRuntime, [switch]$SkipEngineTests, [string]$OutputDirectory="artifacts/MeadowPeaks-win-x64")
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    if (-not $SkipEngineTests) {
        dotnet run --project tests/MeadowPeaks.Tests.csproj -c Release
        if ($LASTEXITCODE -ne 0) { throw 'Engine tests failed.' }
    }
    $output = Join-Path $projectRoot $OutputDirectory
    if ($OfflineInstalledRuntime) {
        dotnet publish src/MeadowPeaks.csproj -c Release --self-contained false -o artifacts/framework-dependent --ignore-failed-sources
        if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
        python scripts/offline_bundle.py artifacts/framework-dependent artifacts/MeadowPeaks-win-x64
        if ($LASTEXITCODE -ne 0) { throw 'Offline runtime bundling failed.' }
    } else {
        dotnet publish src/MeadowPeaks.csproj -c Release -r win-x64 --self-contained true -p:DebugType=None -o $output
        if ($LASTEXITCODE -ne 0) { throw 'Publish failed. For an offline machine, use -OfflineInstalledRuntime.' }
    }
    $env:MEADOW_PEAKS_CACHE=Join-Path $projectRoot 'artifacts/desktop-cache'
    $process = Start-Process -FilePath (Join-Path $output 'MeadowPeaks.exe') -ArgumentList '--smoke' -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(180000)) { $process.Kill(); throw 'UI smoke test timed out.' }
    $result = Get-Content (Join-Path $output 'smoke-result.txt') -Raw
    if ($process.ExitCode -ne 0 -or $result -notlike 'PASS:*') { throw "UI smoke test failed: $result" }
    Write-Output $result
    Copy-Item LICENSE (Join-Path $output 'GAME-LICENSE.txt') -Force
    Copy-Item README.md (Join-Path $output 'README.md') -Force
    Copy-Item docs/PLAYING.txt (Join-Path $output 'PLAYING.txt') -Force
    $docOutput = Join-Path $output 'docs'
    New-Item -ItemType Directory -Path $docOutput -Force | Out-Null
    Get-ChildItem -LiteralPath docs -File | Copy-Item -Destination $docOutput -Force
    # Ship game files and runtime notices; exclude diagnostic screenshots and reports.
    $files = Get-ChildItem -LiteralPath $output -Force | Where-Object { $_.Name -ne 'screenshots' -and $_.Name -ne 'smoke-result.txt' -and $_.Name -ne 'MeadowPeaks-startup.txt' -and $_.Name -ne 'MeadowPeaks-error.txt' -and $_.Extension -ne '.pdb' }
    Compress-Archive -LiteralPath $files.FullName -DestinationPath artifacts/MeadowPeaks-win-x64.zip -Force
    Write-Output "Ready: $projectRoot/artifacts/MeadowPeaks-win-x64.zip"
} finally { Pop-Location }
