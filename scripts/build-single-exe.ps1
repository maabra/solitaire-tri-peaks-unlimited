$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    dotnet publish src/MeadowPeaks.csproj -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true `
        -p:DebugType=None -o artifacts/single-file
    if ($LASTEXITCODE -ne 0) { throw 'Single-file publish failed.' }
    $executable = Join-Path $projectRoot 'artifacts/MeadowPeaks.exe'
    Copy-Item artifacts/single-file/MeadowPeaks.exe $executable -Force
    $process = Start-Process -FilePath $executable -ArgumentList '--smoke' -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(60000)) { $process.Kill(); throw 'Single-file UI smoke test timed out.' }
    $result = Get-Content artifacts/smoke-result.txt -Raw
    if ($process.ExitCode -ne 0 -or $result -notlike 'PASS:*') { throw "Single-file UI test failed: $result" }
    Write-Output $result
    Write-Output "Double-click to play: $executable"
} finally { Pop-Location }
