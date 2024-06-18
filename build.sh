#!/bin/bash

function ShowHelp()
{
	echo
	echo "Usage: $0 [clean|Build|rebuild]"
	echo
	echo "$0 without any arguments is equivalent to $0 build"
	exit
}

function EndWithError()
{
	echo
	echo "** ERROR: Build failed and aborted! **"
	exit
}

#Check parameters
case $1 in 
	help    | -h) ShowHelp;;
	build   | -b) BUILDTYPE="CoreBuild";;
	clean   | -c) BUILDTYPE="clean";;
	rebuild | -r) BUILDTYPE="rebuild";;
	"") BUILDTYPE="CoreBuild";;
	*) echo; echo "Unsupported commandline switch!"; EndWithError
esac

#Search mono, msbuild and nuget
if [ -z  $(command -v "mono") ]; then 
	echo "mono not found!"
	echo "please download from official website <http://www.mono-project.com>";
	EndWithError
fi
if [ -z  $(command -v "msbuild") ]; then 
	echo "msbuild not found!"
	echo "please download mono from official website <http://www.mono-project.com>"
	EndWithError
fi
if [ -z  $(command -v "nuget") ]; then 
	echo "nuget not found!"
	echo "Please install nuget"
	EndWithError
fi

# Enter in correct diretory
if [ ${0%/*} == $0 ]; then
	cd ${PWD}
elif [ -e ${PWD}/${0%/*} ]; then
	cd ${PWD}/${0%/*}
else
	cd ${0%/*}
fi

#Try update souce code
git pull

echo
echo "Starting compilation..."
echo
echo "$BUILDTYPE""ing Subtitle Edit - Release|Any CPU..."
echo Check for new translation strings...
echo
msbuild src/UpdateLanguageFiles/UpdateLanguageFiles.csproj -r -t:Rebuild -p:Configuration=Debug -p:Platform=\"Any CPU\" -p:OutputPath=bin/Debug
echo
 
LanguageToolPath="src/UpdateLanguageFiles/bin/debug/UpdateLanguageFiles.exe"
if [ -e "./"$LanguageToolPath ]; then
	mono $LanguageToolPath LanguageBaseEnglish.xml src/ui/Logic/LanguageDeserializer.cs
else
	echo "Compile UpdateLanguageFiles!"
fi

echo
# Restore nuget package (the '-r' does nothing on my system)
nuget restore SubtitleEdit.sln
echo
msbuild SubtitleEdit.sln -r -t:SubtitleEdit:$BUILDTYPE -p:Configuration=Release -p:Platform=\"Any CPU\" -maxcpucount -consoleloggerparameters:DisableMPLogging\;Summary\;Verbosity=minimal

if [ $? -eq 1 ]; then
	echo EndWithError
fi

echo
echo $BUILDTYPE"ing Subtitle Edit finished!"


