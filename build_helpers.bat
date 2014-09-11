@ECHO OFF
SETLOCAL

PUSHD %~dp0

IF /I "%~1" == "lang" GOTO UpdateLanguageFiles
IF /I "%~1" == "rev"  GOTO UpdateAssemblyInfo


:END
POPD
ENDLOCAL
EXIT /B


:UpdateLanguageFiles
IF NOT EXIST "src\UpdateLanguageFiles\bin\Release\UpdateLanguageFiles.exe" IF NOT EXIST "src\UpdateLanguageFiles\bin\Debug\UpdateLanguageFiles.exe" (
  ECHO Compile Subtitle Edit first!
  GOTO END
)

IF EXIST "src\UpdateLanguageFiles\bin\Release\UpdateLanguageFiles.exe" (
  "src\UpdateLanguageFiles\bin\Release\UpdateLanguageFiles.exe" "LanguageMaster.xml" "src\Logic\LanguageDeserializer.cs"
) ELSE (
  "src\UpdateLanguageFiles\bin\Debug\UpdateLanguageFiles.exe" "LanguageMaster.xml" "src\Logic\LanguageDeserializer.cs"
)

IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR: Something went wrong when generating the language files...
)
GOTO END


:UpdateAssemblyInfo
IF NOT EXIST "src\UpdateAssemblyInfo\bin\Release\UpdateAssemblyInfo.exe" IF NOT EXIST "src\UpdateAssemblyInfo\bin\Debug\UpdateAssemblyInfo.exe" (
  ECHO Compile Subtitle Edit first!
  GOTO END
)

IF EXIST "src\UpdateAssemblyInfo\bin\Release\UpdateAssemblyInfo.exe" (
  "src\UpdateAssemblyInfo\bin\Release\UpdateAssemblyInfo.exe" "src\Properties\AssemblyInfo.cs.template" "src\Properties\AssemblyInfo.cs"
) ELSE (
  "src\UpdateAssemblyInfo\bin\Debug\UpdateAssemblyInfo.exe" "src\Properties\AssemblyInfo.cs.template" "src\Properties\AssemblyInfo.cs"
)

IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR: Something went wrong when generating the revision number...
)
GOTO END
