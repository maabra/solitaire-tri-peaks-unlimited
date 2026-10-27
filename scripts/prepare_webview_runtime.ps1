# Fetch the pinned official renderer into a partial file, then verify its checksum before promoting it.
# An existing verified CAB is reused; interrupted downloads never become the accepted runtime.
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$root=Split-Path -Parent $PSScriptRoot
$destination=Join-Path $root 'artifacts/webview-runtime.cab'
$expected='CB4612CE79A62A0BE094301A72F73CF91BB250F5E83DC51886F66A083FC0694F'
if((Test-Path -LiteralPath $destination) -and (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -eq $expected){Write-Output 'Portable renderer already verified.';return}
New-Item -ItemType Directory -Path (Join-Path $root 'artifacts') -Force | Out-Null
$temporary=Join-Path $root 'artifacts/webview-runtime.cab.partial'
Invoke-WebRequest -UseBasicParsing -Uri 'https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/074c0930-36ad-4756-9d04-dfe0e503bd94/Microsoft.WebView2.FixedVersionRuntime.156.0.4314.42.x64.cab' -OutFile $temporary
if((Get-FileHash -LiteralPath $temporary -Algorithm SHA256).Hash -ne $expected){throw 'Portable renderer checksum mismatch.'}
Move-Item -LiteralPath $temporary -Destination $destination -Force
Write-Output 'Official Microsoft WebView2 156.0.4314.42 x64 runtime verified.'
