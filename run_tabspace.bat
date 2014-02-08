@ECHO OFF
SETLOCAL

PUSHD %~dp0

tabspace /exclude:*\*.designer.cs;*\zlib\*;*\HashSet.cs

POPD
ENDLOCAL
EXIT /B
