#!bin/bash

# Get script folder
if [ ${0%/*} == $0 ]; then
	ScriptPath=${PWD}/
elif [ -e ${PWD}/${0%/*} ]; then
	ScriptPath=${PWD}/${0%/*}/
else
	ScriptPath=${0%/*}/
fi

ConfigurationName="$2"

if [ "$1" == "lang" ]; then  
	ToolPath=$ScriptPath"src/UpdateLanguageFiles/bin/$ConfigurationName/UpdateLanguageFiles.exe"
	if [ -e $ToolPath ]; then
		mono $ToolPath $ScriptPath"LanguageBaseEnglish.xml" $ScriptPath"src/ui/Logic/LanguageDeserializer.cs"
	else
		echo "Compile Subtitle Edit first!"
		echo
	fi
elif [ "$1" == "rev" ]; then 
	ToolPath=$ScriptPath"src/UpdateAssemblyInfo/bin/$ConfigurationName/UpdateAssemblyInfo.exe"
	if [ -e $ToolPath ]; then
		mono $ToolPath $ScriptPath"src/ui/Properties/AssemblyInfo.cs.template" $ScriptPath"src/libse/Properties/AssemblyInfo.cs.template"
	else
		echo "Compile Subtitle Edit first!"
		echo
	fi
fi
