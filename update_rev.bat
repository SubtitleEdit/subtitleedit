@ECHO OFF
SETLOCAL

PUSHD %~dp0

SET "SUBWCREV=SubWCRev.exe"

"%SUBWCREV%" . "src\Properties\AssemblyInfo.cs.in" "src\Properties\AssemblyInfo.cs" -f
IF %ERRORLEVEL% NEQ 0 GOTO NoSubWCRev

GOTO END

:NoSubWCRev
ECHO. & ECHO SubWCRev, which is part of TortoiseSVN, wasn't found!
ECHO You should (re)install TortoiseSVN.
ECHO I'll use VERSION_REV=0 for now.

TYPE "src\Properties\AssemblyInfo.cs.template" > "src\Properties\AssemblyInfo.cs"

:END
POPD
ENDLOCAL
EXIT /B
