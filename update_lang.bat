@ECHO OFF
SETLOCAL

PUSHD %~dp0

IF EXIST "src\UpdateLanguageFiles\bin\Release\UpdateLanguageFiles.exe" (
  "src\UpdateLanguageFiles\bin\Release\UpdateLanguageFiles.exe" "LanguageMaster.xml" "src\Logic\LanguageDeserializer.cs"
) ELSE (
  "src\UpdateLanguageFiles\bin\Debug\UpdateLanguageFiles.exe" "LanguageMaster.xml" "src\Logic\LanguageDeserializer.cs"
)
IF %ERRORLEVEL% NEQ 0 GOTO SubError


:END
POPD
ENDLOCAL
EXIT /B

:SubError
ECHO Something went wrong when generating the language files.
EXIT /B
