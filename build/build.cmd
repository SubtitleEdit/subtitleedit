@echo off
setlocal enabledelayedexpansion

echo =======================================
echo Building Subtitle Edit...
echo =======================================

:: Define output directory in a custom folder to prevent file locking issues
set "OUT_DIR=%~dp0..\src\ui\bin\Release\net10.0-custom"
set "EXE_PATH=!OUT_DIR!\SubtitleEdit.exe"

:: Run dotnet build on the UI project targeting the custom directory
dotnet build "%~dp0..\src\ui\UI.csproj" -c Release -o "!OUT_DIR!"
if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Build failed with exit code %ERRORLEVEL%!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [SUCCESS] Build completed successfully with 0 errors!
echo.

:: Open the output folder
echo Opening folder: !OUT_DIR!
explorer.exe "!OUT_DIR!"

:: Run the executable
if exist "!EXE_PATH!" (
    echo Launching: !EXE_PATH!
    start "" "!EXE_PATH!"
) else (
    echo [WARNING] Could not find SubtitleEdit.exe in !OUT_DIR!
)

endlocal
