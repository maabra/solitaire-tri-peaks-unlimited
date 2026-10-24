param([ValidatePattern("^[A-Za-z0-9._-]+\.exe$")][string]$OutputName = "MeadowPeaks.exe")
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    dotnet publish src/MeadowPeaks.csproj -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true `
        -p:DebugType=None -o artifacts/single-file
    if ($LASTEXITCODE -ne 0) { throw 'Single-file publish failed.' }
    $executable = Join-Path (Join-Path $projectRoot 'artifacts') $OutputName
    Copy-Item artifacts/single-file/MeadowPeaks.exe $executable -Force
    $env:MEADOW_PEAKS_CACHE=Join-Path $projectRoot 'artifacts/desktop-cache'
    $process = Start-Process -FilePath $executable -ArgumentList '--smoke' -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(180000)) { $process.Kill(); throw 'Single-file UI smoke test timed out.' }
    $result = Get-Content artifacts/smoke-result.txt -Raw
    if ($process.ExitCode -ne 0 -or $result -notlike 'PASS:*') { throw "Single-file UI test failed: $result" }
    Write-Output $result
    Write-Output "Double-click to play: $executable"
} finally { Pop-Location }
