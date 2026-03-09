@echo off
setlocal EnableDelayedExpansion

:: ============================================================
::  SubtitleEdit  –  Windows x64 Portable Build
::
::  Output: publish\win-portable\
::
::  Requirements:
::    - .NET 10 SDK  (https://dotnet.microsoft.com/download)
::    - Run from the repository root directory
:: ============================================================

set "OUT=publish\win-portable"
set "PLUGINS_OUT=%OUT%\Plugins"

echo.
echo === SubtitleEdit Portable Build (Windows x64) ===
echo Output: %OUT%
echo.

:: ── 1. Clean previous output ─────────────────────────────────
if exist "%OUT%" (
    echo [1/4] Cleaning previous output...
    rmdir /s /q "%OUT%"
)
mkdir "%PLUGINS_OUT%"

:: ── 2. Publish main application ──────────────────────────────
echo [2/4] Publishing main application (self-contained, win-x64)...
dotnet publish src\UI\UI.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=false ^
    -p:PublishTrimmed=false ^
    --output "%OUT%"

if errorlevel 1 (
    echo.
    echo ERROR: Main application publish failed.
    exit /b 1
)

:: ── 3. Build & copy plugin ────────────────────────────────────
echo [3/4] Building WhisperSampleExport plugin...
dotnet publish src\Plugins\WhisperSampleExport\WhisperSampleExportPlugin.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained false ^
    --output "%PLUGINS_OUT%\WhisperSampleExport_tmp"

if errorlevel 1 (
    echo.
    echo ERROR: Plugin build failed.
    exit /b 1
)

:: Copy only the plugin DLL (host provides everything else)
copy /y "%PLUGINS_OUT%\WhisperSampleExport_tmp\WhisperSampleExport.dll" "%PLUGINS_OUT%\WhisperSampleExport.dll" >nul
rmdir /s /q "%PLUGINS_OUT%\WhisperSampleExport_tmp"

:: ── 4. Remove Linux-only native library ───────────────────────
echo [4/4] Removing Linux-only assets...
if exist "%OUT%\libSkiaSharp.so" del /q "%OUT%\libSkiaSharp.so"

:: ── Done ──────────────────────────────────────────────────────
echo.
echo ============================================================
echo  Build complete!
echo  Portable folder: %CD%\%OUT%
echo.
echo  To run: %OUT%\SubtitleEdit.exe
echo.
echo  To create a ZIP (requires PowerShell):
echo    powershell Compress-Archive -Path '%OUT%\*' -DestinationPath 'SubtitleEdit-win-portable.zip' -Force
echo ============================================================
echo.
endlocal
