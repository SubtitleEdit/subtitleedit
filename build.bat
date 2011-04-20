@ECHO OFF
SETLOCAL
CD /D %~dp0

rem Check for the help switches
IF /I "%~1"=="help"   GOTO SHOWHELP
IF /I "%~1"=="/help"  GOTO SHOWHELP
IF /I "%~1"=="-help"  GOTO SHOWHELP
IF /I "%~1"=="--help" GOTO SHOWHELP
IF /I "%~1"=="/?"     GOTO SHOWHELP

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
TITLE %BUILDTYPE%ing SubtitleEdit with MSVC 2010 - Release^|Any CPU...

"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" SubtitleEdit.sln^
 /t:%BUILDTYPE% /p:Configuration=Release /p:Platform="Any CPU" /maxcpucount^
 /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

ECHO.
POPD

IF /I "%BUILDTYPE%" == "Clean" GOTO END

CALL :SubDetectInnoSetup

IF DEFINED InnoSetupPath (
  PUSHD "installer"

  TITLE Compiling installer...
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


:SHOWHELP
TITLE "%~nx0 %1"
ECHO. & ECHO.
ECHO Usage:   %~nx0 [Clean^|Build^|Rebuild]
ECHO.
ECHO Notes:   You can also prefix the commands with "-", "--" or "/".
ECHO          The arguments are case insesitive.
ECHO. & ECHO.
ECHO Executing "%~nx0" will use the defaults: "%~nx0 build"
ECHO.
ENDLOCAL
EXIT /B


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
