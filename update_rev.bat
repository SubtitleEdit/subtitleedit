@ECHO OFF
SETLOCAL

PUSHD %~dp0

IF EXIST "SubWCRev.exe" SET "SUBWCREV=SubWCRev.exe"
FOR %%A IN (SubWCRev.exe) DO (SET SUBWCREV=%%~$PATH:A)
IF NOT DEFINED SUBWCREV GOTO SubNoSubWCRev

"%SUBWCREV%" . "src\Properties\AssemblyInfo.cs.in" "src\Properties\AssemblyInfo.cs" -f
IF %ERRORLEVEL% NEQ 0 GOTO SubError


:END
POPD
ENDLOCAL
EXIT /B


:SubNoSubWCRev
ECHO. & ECHO SubWCRev, which is part of TortoiseSVN, wasn't found!
ECHO You should (re)install TortoiseSVN.
GOTO SubCommon

:SubError
ECHO Something went wrong when generating the revision number.

:SubCommon
ECHO I'll use VERSION_REV=0 for now.

TYPE "src\Properties\AssemblyInfo.cs.template" > "src\Properties\AssemblyInfo.cs"
GOTO END
