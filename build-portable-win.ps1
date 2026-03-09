#Requires -Version 5.1
<#
.SYNOPSIS
    Builds a self-contained portable Windows x64 package of SubtitleEdit.

.DESCRIPTION
    Publishes the main application and the WhisperSampleExport plugin into
    publish\win-portable, then optionally creates a ZIP archive.

.PARAMETER Zip
    When specified, creates SubtitleEdit-win-portable.zip next to the
    repository root after a successful build.

.EXAMPLE
    .\build-portable-win.ps1
    .\build-portable-win.ps1 -Zip
#>
param(
    [switch]$Zip
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$out        = Join-Path $PSScriptRoot 'publish\win-portable'
$pluginsOut = Join-Path $out 'Plugins'
$zipPath    = Join-Path $PSScriptRoot 'SubtitleEdit-win-portable.zip'

Write-Host ''
Write-Host '=== SubtitleEdit Portable Build (Windows x64) ===' -ForegroundColor Cyan
Write-Host "Output: $out"
Write-Host ''

# ── 1. Clean ────────────────────────────────────────────────────
Write-Host '[1/4] Cleaning previous output...' -ForegroundColor Yellow
if (Test-Path $out) { Remove-Item -Recurse -Force $out }
New-Item -ItemType Directory -Path $pluginsOut | Out-Null

# ── 2. Publish main application ─────────────────────────────────
Write-Host '[2/4] Publishing main application (self-contained, win-x64)...' -ForegroundColor Yellow
dotnet publish src\UI\UI.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    --output $out

if ($LASTEXITCODE -ne 0) { throw 'Main application publish failed.' }

# ── 3. Build & copy plugin ──────────────────────────────────────
Write-Host '[3/4] Building WhisperSampleExport plugin...' -ForegroundColor Yellow
$pluginTmp = Join-Path $pluginsOut 'WhisperSampleExport_tmp'

dotnet publish src\Plugins\WhisperSampleExport\WhisperSampleExportPlugin.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output $pluginTmp

if ($LASTEXITCODE -ne 0) { throw 'Plugin build failed.' }

# Copy only the plugin DLL – the host app supplies all shared assemblies
Copy-Item (Join-Path $pluginTmp 'WhisperSampleExport.dll') `
          (Join-Path $pluginsOut 'WhisperSampleExport.dll') -Force
Remove-Item -Recurse -Force $pluginTmp

# ── 4. Remove Linux-only native libraries ───────────────────────
Write-Host '[4/4] Removing Linux-only assets...' -ForegroundColor Yellow
Get-ChildItem $out -Filter 'libSkiaSharp.so' -Recurse -ErrorAction SilentlyContinue |
    Remove-Item -Force

# ── 5. Optional ZIP ─────────────────────────────────────────────
if ($Zip) {
    Write-Host '[5/5] Creating ZIP archive...' -ForegroundColor Yellow
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path (Join-Path $out '*') -DestinationPath $zipPath
    $size = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
    Write-Host "      $zipPath  ($size MB)" -ForegroundColor Green
}

# ── Done ────────────────────────────────────────────────────────
Write-Host ''
Write-Host ('=' * 60) -ForegroundColor Cyan
Write-Host ' Build complete!' -ForegroundColor Green
Write-Host " Portable folder : $out"
Write-Host " Run             : $out\SubtitleEdit.exe"
if ($Zip) {
    Write-Host " ZIP             : $zipPath"
}
Write-Host ('=' * 60) -ForegroundColor Cyan
Write-Host ''
