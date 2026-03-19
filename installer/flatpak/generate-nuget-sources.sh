#!/bin/bash
# Generates installer/flatpak/nuget-sources.json from packages.lock.json
# using flatpak-dotnet-generator.py (from flatpak-builder-tools).
#
# Run this script whenever NuGet dependencies change, then commit the
# updated nuget-sources.json alongside the manifest.
#
# Requires: python3, dotnet, curl
#
# Usage (from repo root):
#   ./installer/flatpak/generate-nuget-sources.sh
#   ./installer/flatpak/generate-nuget-sources.sh \
#       "src/UI/UI.csproj" \
#       "installer/flatpak/nuget-sources.json"

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

PROJECT_PATH="${1:-src/UI/UI.csproj}"
OUTPUT="${2:-installer/flatpak/nuget-sources.json}"

LOCK_FILE="$REPO_ROOT/src/UI/packages.lock.json"
GENERATOR="$SCRIPT_DIR/flatpak-dotnet-generator.py"
GENERATOR_URL="https://raw.githubusercontent.com/flatpak/flatpak-builder-tools/master/dotnet/flatpak-dotnet-generator.py"

cd "$REPO_ROOT"

# ---------------------------------------------------------------------------
# 1. Restore to ensure packages.lock.json is up-to-date
# ---------------------------------------------------------------------------
echo "Restoring NuGet packages (updating packages.lock.json)..."
dotnet restore "$PROJECT_PATH"

if [ ! -f "$LOCK_FILE" ]; then
    echo "Error: $LOCK_FILE was not created. Make sure RestorePackagesWithLockFile=true is set in the .csproj."
    exit 1
fi

# ---------------------------------------------------------------------------
# 2. Download flatpak-dotnet-generator.py if not present
# ---------------------------------------------------------------------------
if [ ! -f "$GENERATOR" ]; then
    echo "Downloading flatpak-dotnet-generator.py..."
    curl -fsSL -o "$GENERATOR" "$GENERATOR_URL"
fi

# ---------------------------------------------------------------------------
# 3. Generate nuget-sources.json
#    Fetches each NuGet package to compute its SHA256 hash — requires network.
# ---------------------------------------------------------------------------
echo "Generating $OUTPUT (this may take a while)..."
python3 "$GENERATOR" "$OUTPUT" "$LOCK_FILE"

echo ""
echo "✓ Generated: $OUTPUT"
echo "  Commit this file alongside the manifest."
echo "  Re-run this script whenever NuGet dependencies change."

