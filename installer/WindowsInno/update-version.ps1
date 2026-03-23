# Script to update Subtitle_Edit_Installer.iss with version from Se.cs
# Mirrors installer/macBundle/update-plist-version.sh for the Windows/Inno Setup build.
#
# Usage (from repo root):
#   ./installer/WindowsInno/update-version.ps1
#   ./installer/WindowsInno/update-version.ps1 -SeCsPath "src/UI/Logic/Config/Se.cs" -IssPath "installer/WindowsInno/Subtitle_Edit_Installer.iss"

param (
    [string]$SeCsPath = "src/UI/Logic/Config/Se.cs",
    [string]$IssPath  = "installer/WindowsInno/Subtitle_Edit_Installer.iss"
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
# 2. Parse into the three constants used in Subtitle_Edit_Installer.iss
#    "5.0.0-beta4"     -> app_ver="5.0.0"  app_ver_suffix="beta4"   app_ver_full="5.0.0.4"
#    "5.0.0-preview95" -> app_ver="5.0.0"  app_ver_suffix="preview95" app_ver_full="5.0.0.95"
#    "5.1.2"           -> app_ver="5.1.2"  app_ver_suffix=""        app_ver_full="5.1.2.0"
# ---------------------------------------------------------------------------
$parts        = $versionString -split '-', 2
$numericPart  = $parts[0]
$appVerSuffix = if ($parts.Length -gt 1) { $parts[1] } else { "" }   # e.g. "beta4", "preview95", ""

$numericFields = $numericPart.Split('.')
$major = [int]$numericFields[0]
$minor = if ($numericFields.Length -gt 1) { [int]$numericFields[1] } else { 0 }
$build = if ($numericFields.Length -gt 2) { [int]$numericFields[2] } else { 0 }

# Revision: trailing digits of the suffix ("beta4"->4, "preview95"->95, ""->0)
$revisionMatch = [regex]::Match($appVerSuffix, '\d+$')
$revision      = if ($revisionMatch.Success) { [int]$revisionMatch.Value } else { 0 }

$appVer     = "$major.$minor.$build"            # output filename, e.g. "5.0.0"
$appVerFull = "$major.$minor.$build.$revision"  # VersionInfoVersion, e.g. "5.0.0.4"

Write-Host "app_ver        : $appVer"
Write-Host "app_ver_suffix : $appVerSuffix"
Write-Host "app_ver_full   : $appVerFull"

# ---------------------------------------------------------------------------
# 3. Replace the three version constants in the .iss file.
#    app_ver_display is a computed ISPP expression and never needs updating.
# ---------------------------------------------------------------------------
$iss = Get-Content $IssPath -Raw

$iss = $iss -replace '(?m)^#define app_ver\s+.*$',        "#define app_ver         `"$appVer`""
$iss = $iss -replace '(?m)^#define app_ver_suffix\s+.*$', "#define app_ver_suffix  `"$appVerSuffix`""
$iss = $iss -replace '(?m)^#define app_ver_full\s+.*$',   "#define app_ver_full    `"$appVerFull`""

[System.IO.File]::WriteAllText((Resolve-Path $IssPath), $iss)

Write-Host "Successfully updated $IssPath"
Write-Host "  #define app_ver         `"$appVer`""
Write-Host "  #define app_ver_suffix  `"$appVerSuffix`""
Write-Host "  #define app_ver_full    `"$appVerFull`""
