@ECHO OFF
SETLOCAL

PUSHD %~dp0

tabspace /exclude:*\*.designer.cs;*\zlib\*;*\HashSet.cs;*Logic\NHunspell\*

POPD
ENDLOCAL
EXIT /B
