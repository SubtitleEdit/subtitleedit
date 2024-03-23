@ECHO OFF
SETLOCAL
ECHO.
ECHO Validate with: winget validate --manifest C:\git\subtitleedit\winget
ECHO Test with    : winget install --manifest C:\git\subtitleedit\winget
ECHO Submit with  : wingetcreate submit C:\git\subtitleedit\winget
pause