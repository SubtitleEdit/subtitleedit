@ECHO OFF
SETLOCAL

PUSHD %~dp0

IF NOT EXIST "src\UpdateLanguageFiles\bin\Release\UpdateLanguageFiles.exe" IF NOT EXIST "src\UpdateLanguageFiles\bin\Debug\UpdateLanguageFiles.exe" (
  ECHO Compile Subtitle Edit first!
  GOTO END
)

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
ECHO ERROR: Something went wrong when generating the language files...
EXIT /B
