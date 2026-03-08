# Script to update UI.csproj with version from Se.cs
# Mirrors installer/macBundle/update-plist-version.sh for the Windows/Inno Setup build.
#
# Usage (from repo root):
#   ./installer/WindowsInno/update-version.ps1
#   ./installer/WindowsInno/update-version.ps1 -SeCsPath "src/UI/Logic/Config/Se.cs" -CsprojPath "src/UI/UI.csproj"

param (
    [string]$SeCsPath   = "src/UI/Logic/Config/Se.cs",
    [string]$CsprojPath = "src/UI/UI.csproj"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# 1. Extract version string from Se.cs
#    Matches: public static string Version { get; set; } = "v5.0.0-beta4";
# ---------------------------------------------------------------------------
$seContent = Get-Content $SeCsPath -Raw
$match = [regex]::Match($seContent, 'public\s+static\s+string\s+Version\s*\{[^}]+\}\s*=\s*"v([^"]+)"')
if (-not $match.Success) {
    Write-Error "Could not find version line in $SeCsPath"
    exit 1
}

$versionString = $match.Groups[1].Value   # e.g. "5.0.0-beta4"
Write-Host "Extracted version from Se.cs: $versionString"

# ---------------------------------------------------------------------------
# 2. Parse into numeric parts
#    "5.0.0-beta4"   -> major=5, minor=0, build=0, revision=4
#    "5.0.0-preview95" -> major=5, minor=0, build=0, revision=95
#    "5.1.2"          -> major=5, minor=1, build=2, revision=0
# ---------------------------------------------------------------------------
$numericPart   = ($versionString -split '-')[0]           # "5.0.0"
$numericFields = $numericPart.Split('.')
$major   = [int]$numericFields[0]
$minor   = if ($numericFields.Length -gt 1) { [int]$numericFields[1] } else { 0 }
$build   = if ($numericFields.Length -gt 2) { [int]$numericFields[2] } else { 0 }

# Extract trailing digits from suffix, e.g. "beta4" -> 4, "preview95" -> 95
$suffixMatch = [regex]::Match($versionString, '\D(\d+)$')
$revision    = if ($suffixMatch.Success) { [int]$suffixMatch.Groups[1].Value } else { 0 }

# FileVersion / AssemblyVersion: four numeric parts (read by ParseVersion in Inno Setup)
$fileVersion = "$major.$minor.$build.$revision"

# Version: NuGet/display string without the leading "v"
$packageVersion = $versionString

Write-Host "FileVersion / AssemblyVersion : $fileVersion"
Write-Host "Version (package)             : $packageVersion"

# ---------------------------------------------------------------------------
# 3. Inject or update version tags in UI.csproj
# ---------------------------------------------------------------------------
$csproj = Get-Content $CsprojPath -Raw

# Helper: replace existing tag or insert before a known anchor tag
function Set-CsprojProperty {
    param ([string]$xml, [string]$tag, [string]$value, [string]$anchor)
    $pattern     = "(<$tag>)[^<]*(</\s*$tag\s*>)"
    $replacement = "<$tag>$value</$tag>"
    if ($xml -match $pattern) {
        return $xml -replace $pattern, $replacement
    }
    # Insert the new tag on a new line immediately before the anchor tag
    return $xml -replace "(<$anchor>)", "$replacement`n`t`t`$1"
}

$csproj = Set-CsprojProperty $csproj "Version"         $packageVersion "AssemblyName"
$csproj = Set-CsprojProperty $csproj "AssemblyVersion" $fileVersion    "AssemblyName"
$csproj = Set-CsprojProperty $csproj "FileVersion"     $fileVersion    "AssemblyName"

[System.IO.File]::WriteAllText((Resolve-Path $CsprojPath), $csproj)

Write-Host "Successfully updated $CsprojPath"
Write-Host "  <Version>         $packageVersion"
Write-Host "  <AssemblyVersion> $fileVersion"
Write-Host "  <FileVersion>     $fileVersion"
