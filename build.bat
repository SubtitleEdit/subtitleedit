@ECHO OFF
SETLOCAL

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

CALL "%VS120COMNTOOLS%vsvars32.bat" x86
TITLE %BUILDTYPE%ing SubtitleEdit - Release^|Any CPU...

"MSBuild.exe" SubtitleEdit.sln /t:%BUILDTYPE% /p:Configuration=Release /p:Platform="Any CPU"^
 /maxcpucount /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

ECHO.
POPD

IF /I "%BUILDTYPE%" == "Clean" GOTO END

CALL :SubDetectSevenzipPath
IF DEFINED SEVENZIP IF EXIST "%SEVENZIP%" (
  CALL :SubGetVersion
  CALL :SubZipFile
)

CALL :SubDetectInnoSetup
IF DEFINED InnoSetupPath (
  TITLE Compiling installer...
  "%InnoSetupPath%" /O"." /Q "installer\Subtitle_Edit_installer.iss"
  IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

  ECHO. & ECHO Installer compiled successfully!
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
TITLE Creating the ZIP file...
ECHO. & ECHO Creating the ZIP file...
PUSHD "src\bin\Release"
IF EXIST "temp_zip"                                RD /S /Q "temp_zip"
IF NOT EXIST "temp_zip"                            MD "temp_zip"
IF NOT EXIST "temp_zip\Languages"                  MD "temp_zip\Languages"
IF NOT EXIST "temp_zip\Tesseract"                  MD "temp_zip\Tesseract"
IF NOT EXIST "temp_zip\Tesseract\tessdata"         MD "temp_zip\Tesseract\tessdata"
IF NOT EXIST "temp_zip\Tesseract\tessdata\configs" MD "temp_zip\Tesseract\tessdata\configs"

COPY /Y /V "..\..\..\gpl.txt"                            "temp_zip\"
COPY /Y /V "..\..\..\Changelog.txt"                      "temp_zip\"
COPY /Y /V "Hunspellx86.dll"                             "temp_zip\"
COPY /Y /V "Hunspellx64.dll"                             "temp_zip\"
COPY /Y /V "SubtitleEdit.exe"                            "temp_zip\"
COPY /Y /V "Languages\*.xml"                             "temp_zip\Languages\"
COPY /Y /V "..\..\..\Tesseract\msvcp90.dll"              "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\Tesseract\msvcr90.dll"              "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\Tesseract\tesseract.exe"            "temp_zip\Tesseract\"
COPY /Y /V "..\..\..\Tesseract\tessdata\configs\hocr"    "temp_zip\Tesseract\tessdata\configs\"
COPY /Y /V "..\..\..\Tesseract\tessdata\*.traineddata"   "temp_zip\Tesseract\tessdata\"

PUSHD "temp_zip"
START "" /B /WAIT "%SEVENZIP%" a -tzip -mx=9 "SE%VERSION%.zip" * >NUL
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError


ECHO. & ECHO ZIP file created successfully!
MOVE /Y "SE%VERSION%.zip" "..\..\..\.." >NUL
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


:SubGetVersion
FOR /F delims^=^"^ tokens^=2 %%A IN ('FINDSTR /R /C:"AssemblyVersion" "src\Properties\AssemblyInfo.cs.template"') DO (
  rem 3.4.1.[REVNO]
  SET "VERSION=%%A"
)
rem 3.4.1: 0 from the left and -8 chars from the right
SET "VERSION=%VERSION:~0,-8%"
EXIT /B


:SubDetectSevenzipPath
FOR %%G IN (7z.exe) DO (SET "SEVENZIP_PATH=%%~$PATH:G")
IF EXIST "%SEVENZIP_PATH%" (SET "SEVENZIP=%SEVENZIP_PATH%" & EXIT /B)

FOR %%G IN (7za.exe) DO (SET "SEVENZIP_PATH=%%~$PATH:G")
IF EXIST "%SEVENZIP_PATH%" (SET "SEVENZIP=%SEVENZIP_PATH%" & EXIT /B)

FOR /F "tokens=2*" %%A IN (
  'REG QUERY "HKLM\SOFTWARE\7-Zip" /v "Path" 2^>NUL ^| FIND "REG_SZ" ^|^|
   REG QUERY "HKLM\SOFTWARE\Wow6432Node\7-Zip" /v "Path" 2^>NUL ^| FIND "REG_SZ"') DO SET "SEVENZIP=%%B\7z.exe"
EXIT /B


:SubDetectInnoSetup
FOR /F "tokens=5*" %%A IN (
  'REG QUERY "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1" /v "Inno Setup: App Path" 2^>NUL ^| FIND "REG_SZ" ^|^|
   REG QUERY "HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1" /v "Inno Setup: App Path" 2^>NUL ^| FIND "REG_SZ"') DO SET "InnoSetupPath=%%B\ISCC.exe"
EXIT /B
