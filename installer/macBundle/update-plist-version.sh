#!/bin/bash

# Script to update Info.plist with version from Se.cs
# Usage: ./update-plist-version.sh <path-to-Se.cs> <path-to-Info.plist>

set -e

SE_CS_PATH="${1:-src/UI/Logic/Config/Se.cs}"
PLIST_PATH="${2:-installer/macBundle/SubtitleEdit.app/Contents/Info.plist}"

# Extract version from Se.cs
# Example line: public static string Version { get; set; } = "v5.0.0-preview95";
VERSION_LINE=$(grep -E 'public static string Version.*=.*"v[0-9]' "$SE_CS_PATH")
if [ -z "$VERSION_LINE" ]; then
    echo "Error: Could not find version line in $SE_CS_PATH"
    exit 1
fi

# Extract version string (e.g., "v5.0.0-preview95" -> "5.0.0-preview95")
# Using sed instead of grep -P for BSD compatibility
VERSION=$(echo "$VERSION_LINE" | sed -n 's/.*"v\([^"]*\)".*/\1/p')
echo "Extracted version from Se.cs: $VERSION"

# Convert version to CFBundleShortVersionString format
# "5.0.0-preview95" -> "5.0.0-preview.95"
SHORT_VERSION=$(echo "$VERSION" | sed 's/preview/preview./')
echo "CFBundleShortVersionString: $SHORT_VERSION"

# Convert version to CFBundleVersion format (only numbers)
# "5.0.0-preview95" -> "500095"
BUNDLE_VERSION=$(echo "$VERSION" | sed 's/[^0-9]//g')
echo "CFBundleVersion: $BUNDLE_VERSION"

# Update Info.plist
/usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString $SHORT_VERSION" "$PLIST_PATH"
/usr/libexec/PlistBuddy -c "Set :CFBundleVersion $BUNDLE_VERSION" "$PLIST_PATH"

echo "? Successfully updated $PLIST_PATH"
echo "  CFBundleShortVersionString: $SHORT_VERSION"
echo "  CFBundleVersion: $BUNDLE_VERSION"
