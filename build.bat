@ECHO OFF
SETLOCAL
TITLE Compiling Subtitle Edit...

REM Detect if we are running on 64bit WIN and use Wow6432Node, and set the path
REM of Inno Setup accordingly
IF "%PROGRAMFILES(x86)%zzz"=="zzz" (SET "U_=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
) ELSE (
SET "U_=HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
)

SET "I_=Inno Setup"
SET "A_=%I_% 5"
FOR /f "delims=" %%a IN (
  'REG QUERY "%U_%\%A_%_is1" /v "%I_%: App Path"2^>Nul^|FIND "REG_"') DO (
  SET "InnoSetupPath=%%a"&CALL :SubIS %%InnoSetupPath:*Z=%%)

CD /D %~dp0

PUSHD "src"
CALL "%VS100COMNTOOLS%vsvars32.bat" >NUL

devenv SubtitleEdit.sln /Rebuild "Release|Any CPU"
IF %ERRORLEVEL% NEQ 0 GOTO :EndWithError
ECHO.
POPD

PUSHD "InnoSetupScript"
"%InnoSetupPath%\iscc.exe" /Q "Subtitle_Edit_installer.iss"
IF %ERRORLEVEL% NEQ 0 GOTO :EndWithError
ECHO.&&ECHO:Installer compiled successfully!
MOVE /Y "SubtitleEdit-*-setup.exe" ".." >NUL 2>&1
POPD


:END
ECHO.
ENDLOCAL && PAUSE
EXIT /B

:EndWithError
Title Compiling Subtitle Edit [ERROR]
ECHO. && ECHO.
ECHO: **ERROR: Build failed and aborted!**
PAUSE
ENDLOCAL
EXIT

:SubIS
SET InnoSetupPath=%*
GOTO :EOF