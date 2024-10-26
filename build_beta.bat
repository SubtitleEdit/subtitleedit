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

REM MAKE DIR/CLEAN
IF NOT EXIST SubtitleEditBeta (
MD SubtitleEditBeta
)
IF EXIST SubtitleEditBeta\SubtitleEditBeta.zip (
DEL SubtitleEditBeta\SubtitleEditBeta.zip
)

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
ECHO.
ECHO Visual Studio installation path: "%VSINSTALLDIR%"
IF EXIST "%VSINSTALLDIR%MSBuild\15.0\Bin\MSBuild.exe" (
  SET "MSBUILD=%VSINSTALLDIR%MSBuild\15.0\Bin\MSBuild.exe"
) ELSE (
IF EXIST "%VSINSTALLDIR%MSBuild\Current\Bin\MSBuild.exe" (
  SET "MSBUILD=%VSINSTALLDIR%MSBuild\Current\Bin\MSBuild.exe"
) ELSE (
  ECHO Cannot find Visual Studio 2019.
  GOTO EndWithError
))


ECHO Check for new translation strings...
"%MSBUILD%" src\UpdateLanguageFiles\UpdateLanguageFiles.csproj /r /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"^
 /maxcpucount /p:OutputPath=bin\debug /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal
SET "LanguageToolPath=src\UpdateLanguageFiles\bin\debug\UpdateLanguageFiles.exe"
IF NOT EXIST "%LanguageToolPath%" (
  ECHO Compile UpdateLanguageFiles!
)
"%LanguageToolPath%" "LanguageBaseEnglish.xml" "src\ui\Logic\LanguageDeserializer.cs"
ECHO.


"%MSBUILD%" SubtitleEdit.sln /r /t:%BUILDTYPE% /p:Configuration=Release /p:Platform="Any CPU"^
 /maxcpucount /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

IF /I "%BUILDTYPE%" == "Clean" GOTO EndSuccessful

CALL :SubDetectSevenzipPath
IF DEFINED SEVENZIP IF EXIST "%SEVENZIP%" (
  CALL :SubZipFile
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
PUSHD "src\ui\bin\Release\net48"
IF EXIST "temp_zip"                  RD /S /Q "temp_zip"
IF NOT EXIST "temp_zip"              MD "temp_zip"
IF NOT EXIST "temp_zip\Languages"    MD "temp_zip\Languages"
IF NOT EXIST "temp_zip\Dictionaries" MD "temp_zip\Dictionaries"
IF NOT EXIST "temp_zip\Ocr"          MD "temp_zip\Ocr"
IF NOT EXIST "temp_zip\Tesseract302" MD "temp_zip\Tesseract302"
IF NOT EXIST "temp_zip\Icons"        MD "temp_zip\Icons"
IF NOT EXIST "temp_zip\Icons\DefaultTheme"        MD "temp_zip\Icons\DefaultTheme"
IF NOT EXIST "temp_zip\Icons\DefaultTheme\VideoPlayer"        MD "temp_zip\Icons\DefaultTheme\VideoPlayer"
IF NOT EXIST "temp_zip\Icons\DarkTheme"        MD "temp_zip\Icons\DarkTheme"
IF NOT EXIST "temp_zip\Icons\DarkTheme\VideoPlayer"        MD "temp_zip\Icons\DarkTheme\VideoPlayer"
IF NOT EXIST "temp_zip\Icons\Legacy"        MD "temp_zip\Icons\Legacy"
IF NOT EXIST "temp_zip\Icons\Legacy\VideoPlayer"        MD "temp_zip\Icons\Legacy\VideoPlayer"
IF NOT EXIST "temp_zip\Icons\Black"        MD "temp_zip\Icons\Black"
IF NOT EXIST "temp_zip\Icons\Black\VideoPlayer"        MD "temp_zip\Icons\Black\VideoPlayer"

ECHO.
set dest_dir=temp_zip\
set src_root=..\..\..\..\..\
REM Copy specific files
COPY /Y /V "%src_root%LICENSE.txt" "%dest_dir%"
COPY /Y /V "%src_root%Changelog.txt" "%dest_dir%"
COPY /Y /V "%src_root%preview.mkv" "%dest_dir%"
REM Copy all DLL files in current directory
COPY /Y /V "*.dll" "%dest_dir%"
REM Copy additional DLL files from specific directory
COPY /Y /V "%src_root%DLLs\Interop.QuartzTypeLib.dll" "%dest_dir%"
REM Copy executable
COPY /Y /V "SubtitleEdit.exe" "%dest_dir%"
REM Copy other file types into respective directories
COPY /Y /V "Languages\*.xml" "%dest_dir%Languages\"
COPY /Y /V "%src_root%Dictionaries\*.*" "%dest_dir%Dictionaries\"
COPY /Y /V "%src_root%Ocr\*.*" "%dest_dir%Ocr\"
XCOPY /Y /V "%src_root%Tesseract302\*.*" "%dest_dir%Tesseract302\" /S
COPY /Y /V "%src_root%Icons\*.ico" "%dest_dir%Icons\"
COPY /Y /V "%src_root%Icons\DefaultTheme\*.png" "%dest_dir%Icons\DefaultTheme"
COPY /Y /V "%src_root%Icons\DefaultTheme\VideoPlayer\*.png" "%dest_dir%Icons\DefaultTheme\VideoPlayer"
COPY /Y /V "%src_root%Icons\DarkTheme\*.png" "%dest_dir%Icons\DarkTheme"
COPY /Y /V "%src_root%Icons\DarkTheme\VideoPlayer\*.png" "%dest_dir%Icons\DarkTheme\VideoPlayer"
COPY /Y /V "%src_root%Icons\Legacy\*.png" "%dest_dir%Icons\Legacy"
COPY /Y /V "%src_root%Icons\Legacy\VideoPlayer\*.png" "%dest_dir%Icons\Legacy\VideoPlayer"
COPY /Y /V "%src_root%Icons\Black\*.png" "%dest_dir%Icons\Black"
COPY /Y /V "%src_root%Icons\Black\VideoPlayer\*.png" "%dest_dir%Icons\Black\VideoPlayer"

PUSHD "temp_zip"
START "" /B /WAIT "%SEVENZIP%" a -tzip -mx=9 "SubtitleEditBeta.zip" * >NUL
IF %ERRORLEVEL% NEQ 0 GOTO EndWithError

ECHO.
ECHO ZIP archive created successfully!
MOVE /Y "SubtitleEditBeta.zip" "..\..\..\..\..\..\SubtitleEditBeta" >NUL
POPD
IF EXIST "temp_zip" RD /S /Q "temp_zip"
POPD
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