@ECHO OFF
SETLOCAL

SET "VERSION=3.3.15"
SET "SEVENZIP_PATH=%PROGRAMFILES%\7-Zip\7z.exe"

CD /D %~dp0

rem Check for the help switches
IF /I "%~1" == "help"   GOTO SHOWHELP
IF /I "%~1" == "/help"  GOTO SHOWHELP
IF /I "%~1" == "-help"  GOTO SHOWHELP
IF /I "%~1" == "--help" GOTO SHOWHELP
IF /I "%~1" == "/?"     GOTO SHOWHELP

IF NOT DEFINED VS120COMNTOOLS (
  ECHO Visual Studio 2013 wasn't found
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

CALL "%VS120COMNTOOLS%..\..\VC\vcvarsall.bat" x86
TITLE %BUILDTYPE%ing SubtitleEdit - Release^|x86...

"MSBuild.exe" SubtitleEdit.sln /t:%BUILDTYPE% /p:Configuration=Release /p:Platform="x86"^
 /maxcpucount /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

CALL "%VS120COMNTOOLS%..\..\VC\vcvarsall.bat" x86_amd64
TITLE %BUILDTYPE%ing SubtitleEdit - Release^|x64...

"MSBuild.exe" SubtitleEdit.sln /t:%BUILDTYPE% /p:Configuration=Release /p:Platform="x64"^
 /maxcpucount /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

ECHO.
POPD

IF /I "%BUILDTYPE%" == "Clean" GOTO END

IF DEFINED SEVENZIP_PATH IF EXIST "%SEVENZIP_PATH%" CALL :SubZipFile

CALL :SubDetectInnoSetup

IF DEFINED InnoSetupPath (
  PUSHD "installer"

  TITLE Compiling installer...
  "%InnoSetupPath%\iscc.exe" /O.. /Q "Subtitle_Edit_installer.iss"
  IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

  ECHO. & ECHO Installer compiled successfully!
  POPD
) ELSE (
  ECHO Inno Setup wasn't found; the installer wasn't built
)


:END
TITLE Compiling Subtitle Edit finished!
ECHO.
ENDLOCAL
PAUSE
EXIT /B


:SubZipFile
TITLE Creating the x86 ZIP file...
PUSHD "src\bin\Release\x86"
IF EXIST "temp_zip"                                RD /S /Q "temp_zip"
IF NOT EXIST "temp_zip"                            MD "temp_zip"
IF NOT EXIST "temp_zip\Languages"                  MD "temp_zip\Languages"
IF NOT EXIST "temp_zip\Tesseract"                  MD "temp_zip\Tesseract"
IF NOT EXIST "temp_zip\Tesseract\tessdata"         MD "temp_zip\Tesseract\tessdata"
IF NOT EXIST "temp_zip\Tesseract\tessdata\configs" MD "temp_zip\Tesseract\tessdata\configs"

COPY /Y /V "..\..\..\gpl.txt"                               "temp_zip\"
COPY /Y /V "..\..\..\Changelog.txt"                         "temp_zip\"
COPY /Y /V "Interop.QuartzTypeLib.dll"                      "temp_zip\"
COPY /Y /V "Hunspellx86.dll"                                "temp_zip\"
COPY /Y /V "NHunspell.dll"                                  "temp_zip\"
COPY /Y /V "SubtitleEdit.exe"                               "temp_zip\"
COPY /Y /V "Languages\*.xml"                                "temp_zip\Languages\"
COPY /Y /V "..\..\..\..\Tesseract\msvcp90.dll"              "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\..\Tesseract\msvcr90.dll"              "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\..\Tesseract\tesseract.exe"            "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\..\Tesseract\tessdata\configs\hocr"    "temp_zip\Tesseract\tessdata\configs\"
COPY /Y /V "..\..\..\..\Tesseract\tessdata\eng.traineddata" "temp_zip\Tesseract\tessdata\"

PUSHD "temp_zip"
START "" /B /WAIT "%SEVENZIP_PATH%" a -tzip -mx=9 "SE%VERSION%.x86.zip" * >NUL
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError


MOVE /Y "SE%VERSION%.x86.zip" "..\..\..\..\.." >NUL
POPD
IF EXIST "temp_zip" RD /S /Q "temp_zip"
POPD


TITLE Creating the x64 ZIP file...
PUSHD "src\bin\Release\x64"
IF EXIST "temp_zip"                                RD /S /Q "temp_zip"
IF NOT EXIST "temp_zip"                            MD "temp_zip"
IF NOT EXIST "temp_zip\Languages"                  MD "temp_zip\Languages"
IF NOT EXIST "temp_zip\Tesseract"                  MD "temp_zip\Tesseract"
IF NOT EXIST "temp_zip\Tesseract\tessdata"         MD "temp_zip\Tesseract\tessdata"
IF NOT EXIST "temp_zip\Tesseract\tessdata\configs" MD "temp_zip\Tesseract\tessdata\configs"

COPY /Y /V "..\..\..\gpl.txt"                               "temp_zip\"
COPY /Y /V "..\..\..\Changelog.txt"                         "temp_zip\"
COPY /Y /V "Interop.QuartzTypeLib.dll"                      "temp_zip\"
COPY /Y /V "Hunspellx86.dll"                                "temp_zip\"
COPY /Y /V "NHunspell.dll"                                  "temp_zip\"
COPY /Y /V "SubtitleEdit.exe"                               "temp_zip\"
COPY /Y /V "Languages\*.xml"                                "temp_zip\Languages\"
COPY /Y /V "..\..\..\..\Tesseract\msvcp90.dll"              "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\..\Tesseract\msvcr90.dll"              "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\..\Tesseract\tesseract.exe"            "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\..\Tesseract\tessdata\configs\hocr"    "temp_zip\Tesseract\tessdata\configs\"
COPY /Y /V "..\..\..\..\Tesseract\tessdata\eng.traineddata" "temp_zip\Tesseract\tessdata\"

PUSHD "temp_zip"
START "" /B /WAIT "%SEVENZIP_PATH%" a -tzip -mx=9 "SE%VERSION%.x64.zip" * >NUL
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError


MOVE /Y "SE%VERSION%.x64.zip" "..\..\..\..\.." >NUL
POPD
IF EXIST "temp_zip" RD /S /Q "temp_zip"
POPD
EXIT /B


:EndWithError
Title Compiling Subtitle Edit [ERROR]
ECHO. & ECHO.
ECHO  **ERROR: Build failed and aborted!**
PAUSE
ENDLOCAL
EXIT


:SHOWHELP
TITLE %~nx0 %1
ECHO. & ECHO.
ECHO Usage:   %~nx0 [Clean^|Build^|Rebuild]
ECHO.
ECHO Notes:   You can also prefix the commands with "-", "--" or "/".
ECHO          The arguments are not case sensitive.
ECHO. & ECHO.
ECHO Executing %~nx0 without any arguments is equivalent to "%~nx0 build"
ECHO.
ENDLOCAL
EXIT /B


:SubDetectInnoSetup
rem Detect if we are running on 64bit Windows and use Wow6432Node since Inno Setup is
rem a 32-bit application, and set the registry key of Inno Setup accordingly
IF DEFINED PROGRAMFILES(x86) (
  SET "U_=HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
) ELSE (
  SET "U_=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
)

FOR /F "delims=" %%a IN (
  'REG QUERY "%U_%\Inno Setup 5_is1" /v "Inno Setup: App Path"2^>Nul^|FIND "REG_"') DO (
  SET "InnoSetupPath=%%a" & CALL :SubInnoSetup %%InnoSetupPath:*Z=%%)
EXIT /B


:SubInnoSetup
SET "InnoSetupPath=%*"
EXIT /B
