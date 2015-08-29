@ECHO OFF
SETLOCAL
PUSHD %~dp0
SET "ConfigurationName=%~2"

IF /I "%~1" == "lang" GOTO UpdateLanguageFiles
IF /I "%~1" == "rev"  GOTO UpdateAssemblyInfo

:END
POPD
ENDLOCAL
EXIT /B


:UpdateLanguageFiles
IF NOT EXIST "src\UpdateLanguageFiles\bin\%ConfigurationName%\UpdateLanguageFiles.exe" (
  ECHO Compile Subtitle Edit first!
  GOTO END
)

"src\UpdateLanguageFiles\bin\%ConfigurationName%\UpdateLanguageFiles.exe" "LanguageMaster.xml" "src\Logic\LanguageDeserializer.cs"

IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR: Something went wrong when generating the language files...
)
GOTO END


:UpdateAssemblyInfo
IF NOT EXIST "src\UpdateAssemblyInfo\bin\%ConfigurationName%\UpdateAssemblyInfo.exe" (
  ECHO Compile Subtitle Edit first!
  GOTO END
)

"src\UpdateAssemblyInfo\bin\%ConfigurationName%\UpdateAssemblyInfo.exe" "src\Properties\AssemblyInfo.cs.template" "libse\Properties\AssemblyInfo.cs.template"

IF %ERRORLEVEL% NEQ 0 (
  ECHO ERROR: Something went wrong when generating the revision number...
)
GOTO END