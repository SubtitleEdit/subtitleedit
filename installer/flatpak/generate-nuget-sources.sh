#!/bin/bash
# Generates installer/flatpak/nuget-sources.json for use with flatpak-builder.
#
# Preferred method: uses flatpak-dotnet-generator.py from flatpak-builder-tools,
# which restores packages inside the Freedesktop SDK for full reproducibility.
#
# Fallback method: when flatpak (or the required SDK extensions) is not available,
# a built-in Python snippet restores via system dotnet and produces identical output.
#
# Run this script whenever NuGet dependencies change, then commit both
# nuget-sources.json and src/UI/packages.lock.json alongside the manifest.
#
# Requires (preferred): flatpak, org.freedesktop.Sdk//24.08,
#                       org.freedesktop.Sdk.Extension.dotnet10//24.08, python3
# Requires (fallback):  dotnet >= 10, python3
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

GENERATOR="$SCRIPT_DIR/flatpak-dotnet-generator.py"
GENERATOR_URL="https://raw.githubusercontent.com/flatpak/flatpak-builder-tools/master/dotnet/flatpak-dotnet-generator.py"

FREEDESKTOP_VERSION="24.08"
DOTNET_VERSION="10"

cd "$REPO_ROOT"

# ---------------------------------------------------------------------------
# 1. Download flatpak-dotnet-generator.py if not present
# ---------------------------------------------------------------------------
if [ ! -f "$GENERATOR" ]; then
    echo "Downloading flatpak-dotnet-generator.py..."
    curl -fsSL -o "$GENERATOR" "$GENERATOR_URL"
fi

# ---------------------------------------------------------------------------
# 2. Check if flatpak + required SDK extensions are available
# ---------------------------------------------------------------------------
FLATPAK_OK=false
if command -v flatpak &>/dev/null; then
    if flatpak info "org.freedesktop.Sdk.Extension.dotnet${DOTNET_VERSION}//${FREEDESKTOP_VERSION}" &>/dev/null; then
        FLATPAK_OK=true
    fi
fi

# ---------------------------------------------------------------------------
# 3a. Preferred path: use flatpak-dotnet-generator.py (needs flatpak + SDK)
# ---------------------------------------------------------------------------
if [ "$FLATPAK_OK" = "true" ]; then
    echo "Using flatpak-dotnet-generator.py (preferred)..."
    # NOTE: positional args (output, project) must come BEFORE --runtime to avoid
    # argparse nargs='+' greedily consuming them as additional runtime values.
    python3 "$GENERATOR" \
        "$OUTPUT" \
        "$PROJECT_PATH" \
        --dotnet  "$DOTNET_VERSION" \
        --freedesktop "$FREEDESKTOP_VERSION" \
        --runtime linux-x64 \
        --runtime linux-arm64

# ---------------------------------------------------------------------------
# 3b. Fallback path: restore via system dotnet, same JSON output
# ---------------------------------------------------------------------------
else
    echo "flatpak SDK not available \u2014 using system dotnet fallback..."
    echo "(Install flatpak + org.freedesktop.Sdk.Extension.dotnet10//${FREEDESKTOP_VERSION} for the preferred method.)"

    if ! command -v dotnet &>/dev/null; then
        echo "Error: dotnet not found. Please install .NET 10 SDK."
        exit 1
    fi

    TMPDIR_PKGS="$(mktemp -d)"
    trap 'rm -rf "$TMPDIR_PKGS"' EXIT

    echo "Restoring for linux-x64..."
    dotnet restore "$PROJECT_PATH" \
        --packages "$TMPDIR_PKGS" \
        --runtime linux-x64 \
        -p:SelfContained=true \
        --verbosity quiet

    echo "Restoring for linux-arm64..."
    dotnet restore "$PROJECT_PATH" \
        --packages "$TMPDIR_PKGS" \
        --runtime linux-arm64 \
        -p:SelfContained=true \
        --verbosity quiet

    echo "Building $OUTPUT from restored packages..."
    python3 - "$TMPDIR_PKGS" "$OUTPUT" << 'PYTHON'
import sys, json, base64, binascii
from pathlib import Path

pkgs_dir = Path(sys.argv[1])
output   = sys.argv[2]

sources = []
for sha_file in pkgs_dir.glob("**/*.nupkg.sha512"):
    version = sha_file.parent.name
    name    = sha_file.parent.parent.name
    filename = f"{name}.{version}.nupkg"
    url = f"https://api.nuget.org/v3-flatcontainer/{name}/{version}/{filename}"
    sha512_b64 = sha_file.read_text().strip()
    sha512_hex = binascii.hexlify(base64.b64decode(sha512_b64)).decode("ascii")
    sources.append({
        "type": "file",
        "url": url,
        "sha512": sha512_hex,
        "dest": "nuget-sources",
        "dest-filename": filename,
    })

sources.sort(key=lambda s: s["dest-filename"])
Path(output).write_text(json.dumps(sources, indent=4) + "\n", encoding="utf-8")
print(f"  Written {len(sources)} sources to {output}")
PYTHON
fi

# ---------------------------------------------------------------------------
# 4. Ensure x64 runtime packs are present
#
# flatpak-dotnet-generator.py runs dotnet restore inside the x64 SDK
# container, so x64 runtime packs are already on disk and never downloaded —
# they therefore don't appear in the generated JSON.  The flatpak-builder
# sandbox has no such pre-installed packs, so the build would fail.
# This step detects any arm64 runtime/host packs in the output and adds
# the matching x64 (and arch-neutral ILLink.Tasks) entries if missing.
# ---------------------------------------------------------------------------
python3 - "$OUTPUT" << 'PYTHON'
import sys, json, urllib.request, hashlib

output = sys.argv[1]
data   = json.load(open(output))
existing = {s["dest-filename"] for s in data}

def nuget_entry(name, version):
    filename = f"{name}.{version}.nupkg"
    if filename in existing:
        return None
    url = f"https://api.nuget.org/v3-flatcontainer/{name}/{version}/{filename}"
    print(f"  Fetching missing package: {filename}", flush=True)
    with urllib.request.urlopen(url) as r:
        content = r.read()
    return {
        "type": "file",
        "url": url,
        "sha512": hashlib.sha512(content).hexdigest(),
        "dest": "nuget-sources",
        "dest-filename": filename,
    }

added = []
for src in data:
    fn = src["dest-filename"]
    # Detect arm64 runtime/host packs and derive x64 counterpart
    for arm64_suffix in [".runtime.linux-arm64.", ".app.host.linux-arm64."]:
        if arm64_suffix in fn:
            # e.g. microsoft.netcore.app.runtime.linux-arm64.10.0.5.nupkg
            #   -> microsoft.netcore.app.runtime.linux-x64.10.0.5.nupkg
            x64_fn = fn.replace("linux-arm64", "linux-x64")
            if x64_fn not in existing and x64_fn not in {e["dest-filename"] for e in added}:
                name_ver = x64_fn.removesuffix(".nupkg")
                # split off version (last component after the package name)
                parts = name_ver.split(".")
                # version is the last 3 numeric components (e.g. 10.0.5)
                version = ".".join(parts[-3:])
                name    = ".".join(parts[:-3])
                entry = nuget_entry(name, version)
                if entry:
                    added.append(entry)

# Derive ILLink.Tasks version from netcore runtime pack version
for src in data:
    fn = src["dest-filename"]
    if fn.startswith("microsoft.netcore.app.runtime.linux-arm64."):
        version = fn.removeprefix("microsoft.netcore.app.runtime.linux-arm64.").removesuffix(".nupkg")
        entry = nuget_entry("microsoft.net.illink.tasks", version)
        if entry:
            added.append(entry)
        break

if added:
    data.extend(added)
    data.sort(key=lambda s: s["dest-filename"])
    open(output, "w").write(json.dumps(data, indent=4) + "\n")
    print(f"  Added {len(added)} missing x64 package(s).")
else:
    print("  All x64 runtime packages already present.")
PYTHON

echo ""
echo "\u2713 Generated: $OUTPUT"
echo "  Commit this file alongside src/UI/packages.lock.json and the manifest."
echo "  Re-run this script whenever NuGet dependencies change."
