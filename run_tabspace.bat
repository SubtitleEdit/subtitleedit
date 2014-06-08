@ECHO OFF
SETLOCAL

PUSHD %~dp0

tabspace /ext:c;cc;cpp;cs;cxx;h;hpp;hxx;xml /exclude:*\*.designer.cs;*\zlib\*;*\HashSet.cs;*Logic\NHunspell\*

POPD
ENDLOCAL
EXIT /B
