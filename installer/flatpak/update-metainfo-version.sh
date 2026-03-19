#!/bin/bash
# Script to update dk.nikse.subtitleedit.metainfo.xml with version from Se.cs.
# Mirrors installer/WindowsInno/update-version.ps1 and
#         installer/macBundle/update-plist-version.sh for the Flatpak build.
#
# Usage (from repo root):
#   ./installer/flatpak/update-metainfo-version.sh
#   ./installer/flatpak/update-metainfo-version.sh \
#       "src/UI/Logic/Config/Se.cs" \
#       "installer/flatpak/dk.nikse.subtitleedit.metainfo.xml"

set -e

SE_CS_PATH="${1:-src/UI/Logic/Config/Se.cs}"
METAINFO_PATH="${2:-installer/flatpak/dk.nikse.subtitleedit.metainfo.xml}"

# ---------------------------------------------------------------------------
# 1. Extract version string from Se.cs
#    Matches: public static string Version { get; set; } = "v5.0.0-beta9";
# ---------------------------------------------------------------------------
VERSION_LINE=$(grep -E 'public static string Version.*=.*"v[0-9]' "$SE_CS_PATH")
if [ -z "$VERSION_LINE" ]; then
    echo "Error: Could not find version line in $SE_CS_PATH"
    exit 1
fi

VERSION=$(echo "$VERSION_LINE" | sed -n 's/.*"v\([^"]*\)".*/\1/p')
echo "Extracted version from Se.cs: $VERSION"

TODAY=$(date +%Y-%m-%d)
echo "Release date: $TODAY"

# ---------------------------------------------------------------------------
# 2. Replace the first <release version="..." date="..."> element.
#    Uses a temp file so it works on both GNU (Linux) and BSD (macOS) sed.
# ---------------------------------------------------------------------------
TMP=$(mktemp)
sed "s|<release version=\"[^\"]*\" date=\"[^\"]*\">|<release version=\"$VERSION\" date=\"$TODAY\">|" \
    "$METAINFO_PATH" > "$TMP"
mv "$TMP" "$METAINFO_PATH"

echo "✓ Successfully updated $METAINFO_PATH"
echo "  version : $VERSION"
echo "  date    : $TODAY"

