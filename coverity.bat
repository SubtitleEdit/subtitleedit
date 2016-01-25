@ECHO OFF

SETLOCAL

PUSHD %~dp0

IF NOT DEFINED COVDIR SET "COVDIR=H:\progs\thirdparty\cov-analysis-win64"
IF DEFINED COVDIR IF NOT EXIST "%COVDIR%" (
  ECHO.
  ECHO ERROR: Coverity not found in "%COVDIR%"
  GOTO End
)


CALL "%VS140COMNTOOLS%\vsvars32.bat"
IF %ERRORLEVEL% NEQ 0 (
  ECHO vsvars32.bat call failed.
  GOTO End
)


:Cleanup
IF EXIST "cov-int" RD /q /s "cov-int"
IF EXIST "SubtitleEdit.lzma" DEL "SubtitleEdit.lzma"
IF EXIST "SubtitleEdit.tar"  DEL "SubtitleEdit.tar"
IF EXIST "SubtitleEdit.tgz"  DEL "SubtitleEdit.tgz"


:Main
SET MSBUILD_SWITCHES=/nologo /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"^
 /maxcpucount /consoleloggerparameters:DisableMPLogging;Summary;Verbosity=minimal

"%COVDIR%\bin\cov-build.exe" --dir cov-int MSBuild.exe src\SubtitleEdit.sln %MSBUILD_SWITCHES%


:tar
tar --version 1>&2 2>NUL || (ECHO. & ECHO ERROR: tar not found & GOTO SevenZip)
tar caf "SubtitleEdit.lzma" "cov-int"
GOTO End


:SevenZip
CALL :SubDetectSevenzipPath

rem Coverity is totally bogus with lzma...
rem And since I cannot replicate the arguments with 7-Zip, just use tar/gzip.
IF EXIST "%SEVENZIP%" (
  "%SEVENZIP%" a -ttar "SubtitleEdit.tar" "cov-int"
  "%SEVENZIP%" a -tgzip "SubtitleEdit.tgz" "SubtitleEdit.tar"
  IF EXIST "SubtitleEdit.tar" DEL "SubtitleEdit.tar"
  GOTO End
)


:SubDetectSevenzipPath
FOR %%G IN (7z.exe) DO (SET "SEVENZIP_PATH=%%~$PATH:G")
IF EXIST "%SEVENZIP_PATH%" (SET "SEVENZIP=%SEVENZIP_PATH%" & EXIT /B)

FOR %%G IN (7za.exe) DO (SET "SEVENZIP_PATH=%%~$PATH:G")
IF EXIST "%SEVENZIP_PATH%" (SET "SEVENZIP=%SEVENZIP_PATH%" & EXIT /B)

FOR /F "tokens=2*" %%A IN (
  'REG QUERY "HKLM\SOFTWARE\7-Zip" /v "Path" 2^>NUL ^| FIND "REG_SZ" ^|^|
   REG QUERY "HKLM\SOFTWARE\Wow6432Node\7-Zip" /v "Path" 2^>NUL ^| FIND "REG_SZ"') DO SET "SEVENZIP=%%B\7z.exe"
EXIT /B


:End
POPD
ECHO. & ECHO Press any key to close this window...
PAUSE >NUL
ENDLOCAL
EXIT /B
