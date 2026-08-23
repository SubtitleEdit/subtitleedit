#!/bin/bash
# Script to update dk.nikse.subtitleedit.metainfo.xml with version from Se.cs.
# Mirrors installer/WindowsInno/update-version.ps1 and
#         installer/macBundle/update-plist-version.sh for the Flatpak build.
#
# Behaviour:
#   * If <releases> already has an entry for the version in Se.cs, only that
#     entry's date is refreshed. Re-running is a no-op on an unchanged day.
#   * Otherwise a new entry is inserted at the top of <releases>, keeping every
#     older entry intact. Pre-release versions (any version with a "-" in it)
#     get type="development" so software centres do not offer them as the
#     newest stable.
#
# It deliberately never rewrites an older entry: AppStream <releases> is the
# release history a software centre shows, and an earlier version of this
# script had an unanchored sed that stamped the current version and date onto
# *every* <release> element, collapsing the whole history into duplicates of
# the version being built.
#
# A freshly inserted entry has no <description>. That is valid AppStream, and
# for a stable release it is the one thing worth filling in by hand afterwards
# (see the existing entries for the shape).
#
# Usage (from repo root):
#   ./installer/flatpak/update-metainfo-version.sh
#   ./installer/flatpak/update-metainfo-version.sh \
#       "src/ui/Logic/Config/Se.cs" \
#       "installer/flatpak/dk.nikse.subtitleedit.metainfo.xml"
#
#   ./installer/flatpak/update-metainfo-version.sh --check [se.cs] [metainfo.xml]
#       Verify only, change nothing: exits non-zero unless <releases> already
#       carries an entry for the version in Se.cs. Used by the release workflow
#       to stop a stable release whose metainfo was never updated (the v5.1.0
#       release shipped with 5.0.0 as its newest entry that way).

set -e

CHECK_ONLY=0
if [ "$1" = "--check" ]; then
    CHECK_ONLY=1
    shift
fi

SE_CS_PATH="${1:-src/ui/Logic/Config/Se.cs}"
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

if ! grep -q '<releases>' "$METAINFO_PATH"; then
    echo "Error: No <releases> element found in $METAINFO_PATH"
    exit 1
fi

# ---------------------------------------------------------------------------
# 2. Does <releases> already describe this exact version?
# ---------------------------------------------------------------------------
if grep -q "<release version=\"$VERSION\"" "$METAINFO_PATH"; then
    HAS_ENTRY=1
else
    HAS_ENTRY=0
fi

if [ "$CHECK_ONLY" = "1" ]; then
    if [ "$HAS_ENTRY" = "1" ]; then
        echo "✓ $METAINFO_PATH already has a <release> entry for $VERSION"
        exit 0
    fi
    echo "Error: $METAINFO_PATH has no <release> entry for $VERSION."
    echo "       Add one (newest first) before tagging - a Flathub build takes"
    echo "       its metainfo from the tagged checkout, so whatever is committed"
    echo "       at the tag is what the published app advertises."
    echo "       Newest entry currently in the file:"
    grep -m1 '<release version=' "$METAINFO_PATH" | sed 's/^/         /'
    exit 1
fi

TODAY=$(date +%Y-%m-%d)
echo "Release date: $TODAY"

# ---------------------------------------------------------------------------
# 3. Refresh that entry's date, or insert a new newest entry.
#    awk, not sed: the insert needs multi-line output, and awk behaves the same
#    on GNU (Linux/CI) and BSD (macOS) without the -i portability dance.
# ---------------------------------------------------------------------------
TMP=$(mktemp)

if [ "$HAS_ENTRY" = "1" ]; then
    awk -v ver="$VERSION" -v today="$TODAY" '
        index($0, "<release version=\"" ver "\"") {
            sub(/date="[^"]*"/, "date=\"" today "\"")
        }
        { print }
    ' "$METAINFO_PATH" > "$TMP"
    ACTION="updated the date on the existing entry"
else
    # A version with a "-" is a beta/rc; mark it so it is not treated as stable.
    case "$VERSION" in
        *-*) TYPE_ATTR=' type="development"' ;;
        *)   TYPE_ATTR='' ;;
    esac

    awk -v ver="$VERSION" -v today="$TODAY" -v type_attr="$TYPE_ATTR" '
        { print }
        !inserted && /<releases>/ {
            printf "    <release version=\"%s\"%s date=\"%s\">\n", ver, type_attr, today
            printf "      <url>https://github.com/SubtitleEdit/subtitleedit/releases/tag/v%s</url>\n", ver
            printf "    </release>\n"
            inserted = 1
        }
    ' "$METAINFO_PATH" > "$TMP"
    ACTION="inserted a new newest entry"
fi

if ! grep -q "<release version=\"$VERSION\"" "$TMP"; then
    rm -f "$TMP"
    echo "Error: failed to write a <release> entry for $VERSION - $METAINFO_PATH left unchanged"
    exit 1
fi

mv "$TMP" "$METAINFO_PATH"

echo "✓ Successfully updated $METAINFO_PATH ($ACTION)"
echo "  version : $VERSION"
echo "  date    : $TODAY"
