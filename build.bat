@ECHO OFF
SETLOCAL
TITLE Compiling Subtitle Edit...

CD /D %~dp0

IF NOT DEFINED VS100COMNTOOLS (
  ECHO Visual Studio 2010 wasn't found
  GOTO EndWithError
)

IF "%~1" == "" (
  SET "BUILDTYPE=Build"
) ELSE (
  IF /I "%~1" == "Build"     SET "BUILDTYPE=Build"   & GOTO START
  IF /I "%~1" == "/Build"    SET "BUILDTYPE=Build"   & GOTO START
  IF /I "%~1" == "-Build"    SET "BUILDTYPE=Build"   & GOTO START
  IF /I "%~1" == "--Build"   SET "BUILDTYPE=Build"   & GOTO START
  IF /I "%~1" == "Clean"     SET "BUILDTYPE=Clean"   & GOTO START
  IF /I "%~1" == "/Clean"    SET "BUILDTYPE=Clean"   & GOTO START
  IF /I "%~1" == "-Clean"    SET "BUILDTYPE=Clean"   & GOTO START
  IF /I "%~1" == "--Clean"   SET "BUILDTYPE=Clean"   & GOTO START
  IF /I "%~1" == "Rebuild"   SET "BUILDTYPE=Rebuild" & GOTO START
  IF /I "%~1" == "/Rebuild"  SET "BUILDTYPE=Rebuild" & GOTO START
  IF /I "%~1" == "-Rebuild"  SET "BUILDTYPE=Rebuild" & GOTO START
  IF /I "%~1" == "--Rebuild" SET "BUILDTYPE=Rebuild" & GOTO START

  ECHO. & ECHO Unsupported commandline switch!
  GOTO EndWithError
)


:START
PUSHD "src"

CALL "%VS100COMNTOOLS%vsvars32.bat"

devenv SubtitleEdit.sln /%BUILDTYPE% "Release|Any CPU"
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

ECHO.
POPD

IF /I "%BUILDTYPE%" == "Clean" GOTO END

CALL :SubDetectInnoSetup

IF DEFINED InnoSetupPath (
  PUSHD "installer"

  "%InnoSetupPath%\iscc.exe" /Q "Subtitle_Edit_installer.iss"
  IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

  ECHO. & ECHO Installer compiled successfully!
  MOVE /Y "SubtitleEdit-*-setup.exe" ".." >NUL 2>&1
  POPD
) ELSE (
  ECHO Inno Setup wasn't found; the installer wasn't built
)


:END
ECHO.
ENDLOCAL
PAUSE
EXIT /B


:EndWithError
Title Compiling Subtitle Edit [ERROR]
ECHO. & ECHO.
ECHO  **ERROR: Build failed and aborted!**
PAUSE
ENDLOCAL
EXIT


:SubDetectInnoSetup
REM Detect if we are running on 64bit WIN and use Wow6432Node, and set the path
REM of Inno Setup accordingly
IF "%PROGRAMFILES(x86)%zzz"=="zzz" (
  SET "U_=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
) ELSE (
  SET "U_=HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
)

FOR /F "delims=" %%a IN (
  'REG QUERY "%U_%\Inno Setup 5_is1" /v "Inno Setup: App Path"2^>Nul^|FIND "REG_"') DO (
  SET "InnoSetupPath=%%a" & CALL :SubInnoSetup %%InnoSetupPath:*Z=%%)
EXIT /B


:SubInnoSetup
SET InnoSetupPath=%*
EXIT /B
