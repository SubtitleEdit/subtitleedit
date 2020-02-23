@ECHO OFF
SETLOCAL

CD /D %~dp0

REM Check for the help switches
IF /I "%~1" == "help"   GOTO ShowHelp
IF /I "%~1" == "/help"  GOTO ShowHelp
IF /I "%~1" == "-help"  GOTO ShowHelp
IF /I "%~1" == "--help" GOTO ShowHelp
IF /I "%~1" == "/?"     GOTO ShowHelp
IF /I "%~1" == "-?"     GOTO ShowHelp

ECHO Getting latest changes...
git pull
ECHO.

ECHO Starting compilation...

REM Set environment variables for Visual Studio command line if necessary
:SetVsCmdLineEnv
IF DEFINED VSINSTALLDIR IF EXIST "%VSINSTALLDIR%" (
  IF DEFINED VisualStudioVersion IF "%VisualStudioVersion%" GEQ "15.0" GOTO SetBuildType
  ECHO.
  ECHO Developer Command Prompt for Visual Studio 2017 or later required!
  GOTO EndWithError
)
FOR /F "usebackq delims=" %%A IN (`vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath`) DO (
  SET "VSINSTALLDIR=%%A\"
)
IF DEFINED VSINSTALLDIR IF EXIST "%VSINSTALLDIR%" (
  ECHO Visual Studio installation path: "%VSINSTALLDIR%"
) ELSE (
  ECHO.
  ECHO Cannot find Visual Studio.
  GOTO EndWithError
)
SET "VsDevCmd_bat=%VSINSTALLDIR%Common7\Tools\VsDevCmd.bat"
IF NOT EXIST "%VsDevCmd_bat%" (
  FOR /F "usebackq delims=" %%A IN (`DIR /B /S "%VSINSTALLDIR%\VsDevCmd.bat"`) DO (
    SET "VsDevCmd_bat=%%A"
  )
)
IF EXIST "%VsDevCmd_bat%" (
  ECHO Visual Studio VsDevCmd.bat path: "%VsDevCmd_bat%"
) ELSE (
  ECHO.
  ECHO Cannot find Visual Studio VsDevCmd batch file.
  GOTO EndWithError
)
ECHO.
CALL "%VsDevCmd_bat%" -startdir=none
GOTO SetVsCmdLineEnv


:SetBuildType
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

  ECHO.
  ECHO Unsupported commandline switch!
  GOTO EndWithError
)


:START
TITLE %BUILDTYPE%ing Subtitle Edit - Release^|Any CPU...
ECHO.
ECHO %BUILDTYPE%ing Subtitle Edit - Release^|Any CPU...
DEL /F /Q SubtitleEdit-*-Setup.exe SubtitleEdit-*.zip 2>NUL
PUSHD "src"
ECHO.
ECHO Visual Studio installation path: "%VSINSTALLDIR%"
IF EXIST "%VSINSTALLDIR%MSBuild\15.0\Bin\MSBuild.exe" (
  SET "MSBUILD=%VSINSTALLDIR%MSBuild\15.0\Bin\MSBuild.exe"
) ELSE (
IF EXIST "%VSINSTALLDIR%MSBuild\Current\Bin\MSBuild.exe" (
  SET "MSBUILD=%VSINSTALLDIR%MSBuild\Current\Bin\MSBuild.exe"
) ELSE (
  ECHO Cannot find Visual Studio 2017.
  GOTO EndWithError
))
"%MSBUILD%" SubtitleEdit.sln /r /t:%BUILDTYPE% /p:Configuration=Release /p:Platform="Any CPU"^
 /maxcpucount /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

IF /I "%BUILDTYPE%" == "Clean" GOTO EndSuccessful

ECHO.
ECHO Merging assemblies with ILRepack...
FOR /D %%A IN (packages\ILRepack.*) DO (SET "ILREPACKDIR=%%A")
ECHO.
"%ILREPACKDIR%\tools\ILRepack.exe" /parallel /internalize /targetplatform:v4 /out:"bin\Release\SubtitleEdit.exe" "bin\Release\SubtitleEdit.exe"^
 "bin\Release\libse.dll" "bin\Release\zlib.net.dll" "bin\Release\NHunspell.dll" "bin\Release\UtfUnknown.dll" "DLLs\Interop.QuartzTypeLib.dll"
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError
POPD

CALL :SubDetectSevenzipPath
IF DEFINED SEVENZIP IF EXIST "%SEVENZIP%" (
  CALL :SubGetVersion
  CALL :SubZipFile
)

CALL :SubDetectInnoSetup
IF DEFINED INNOSETUP IF EXIST "%INNOSETUP%" (
  TITLE Compiling installer with Inno Setup...
  ECHO.
  ECHO Compiling installer with Inno Setup...
  "%INNOSETUP%" /O"." /Q "installer\Subtitle_Edit_installer.iss"
  IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

  ECHO.
  ECHO Installer compiled successfully!
) ELSE (
  ECHO Inno Setup wasn't found; the installer wasn't built.
)


:EndSuccessful
TITLE %BUILDTYPE%ing Subtitle Edit finished!
GOTO END


:EndWithError
TITLE Compiling Subtitle Edit [ERROR]
ECHO.
ECHO.
ECHO  ** ERROR: Build failed and aborted! **
GOTO END


:ShowHelp
TITLE %~nx0 %1
ECHO.
ECHO.
ECHO Usage:   %~nx0 [Clean^|Build^|Rebuild]
ECHO.
ECHO Notes:   You can also prefix the commands with "-", "--" or "/".
ECHO          The arguments are not case sensitive.
ECHO.
ECHO Executing %~nx0 without any arguments is equivalent to "%~nx0 build".


:END
ECHO.
ECHO.
ENDLOCAL
PAUSE
EXIT /B


:SubZipFile
TITLE Creating ZIP archive with 7-Zip...
ECHO.
ECHO Creating ZIP archive with 7-Zip...
PUSHD "src\bin\Release"
IF EXIST "temp_zip"                  RD /S /Q "temp_zip"
IF NOT EXIST "temp_zip"              MD "temp_zip"
IF NOT EXIST "temp_zip\Languages"    MD "temp_zip\Languages"
IF NOT EXIST "temp_zip\Dictionaries" MD "temp_zip\Dictionaries"
IF NOT EXIST "temp_zip\Ocr"          MD "temp_zip\Ocr"

ECHO.
COPY /Y /V "..\..\..\LICENSE.txt"      "temp_zip\"
COPY /Y /V "..\..\..\Changelog.txt"    "temp_zip\"
COPY /Y /V "Hunspellx86.dll"           "temp_zip\"
COPY /Y /V "Hunspellx64.dll"           "temp_zip\"
COPY /Y /V "SubtitleEdit.exe"          "temp_zip\"
COPY /Y /V "Languages\*.xml"           "temp_zip\Languages\"
COPY /Y /V "..\..\..\Dictionaries\*.*" "temp_zip\Dictionaries\"
COPY /Y /V "..\..\..\Ocr\*.*"          "temp_zip\Ocr\"

PUSHD "temp_zip"
START "" /B /WAIT "%SEVENZIP%" a -tzip -mx=9 "SubtitleEdit-%VERSION%.zip" * >NUL
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

ECHO.
ECHO ZIP archive created successfully!
MOVE /Y "SubtitleEdit-%VERSION%.zip" "..\..\..\.." >NUL
POPD
IF EXIST "temp_zip" RD /S /Q "temp_zip"
POPD

EXIT /B


:SubGetVersion
FOR /F delims^=^"^ tokens^=2 %%A IN ('FINDSTR /R /C:"AssemblyVersion" "src\Properties\AssemblyInfo.cs.template"') DO (
  REM 3.4.1.[REVNO]
  SET "VERSION=%%A"
)
REM 3.4.1: 0 from the left and -8 chars from the right
SET "VERSION=%VERSION:~0,-8%"
EXIT /B


:SubDetectSevenzipPath
FOR %%G IN (7z.exe) DO (SET "SEVENZIP_PATH=%%~$PATH:G")
IF EXIST "%SEVENZIP_PATH%" (SET "SEVENZIP=%SEVENZIP_PATH%" & EXIT /B)

FOR %%G IN (7za.exe) DO (SET "SEVENZIP_PATH=%%~$PATH:G")
IF EXIST "%SEVENZIP_PATH%" (SET "SEVENZIP=%SEVENZIP_PATH%" & EXIT /B)

FOR /F "tokens=2*" %%A IN (
  'REG QUERY "HKLM\SOFTWARE\7-Zip" /v "Path" 2^>NUL ^|^|
   REG QUERY "HKLM\SOFTWARE\Wow6432Node\7-Zip" /v "Path" 2^>NUL') DO IF "%%A" == "REG_SZ" SET "SEVENZIP=%%B\7z.exe"
EXIT /B


:SubDetectInnoSetup
FOR %%G IN (ISCC.exe) DO (SET "INNOSETUP_PATH=%%~$PATH:G")
IF EXIST "%INNOSETUP_PATH%" (SET "INNOSETUP=%INNOSETUP_PATH%" & EXIT /B)

FOR /F "tokens=5*" %%A IN (
  'REG QUERY "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1" /v "Inno Setup: App Path" 2^>NUL ^|^|
   REG QUERY "HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 5_is1" /v "Inno Setup: App Path" 2^>NUL') DO IF "%%A" == "REG_SZ" SET "INNOSETUP=%%B\ISCC.exe"
EXIT /B
