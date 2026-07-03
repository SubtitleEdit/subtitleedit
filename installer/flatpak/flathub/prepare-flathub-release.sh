#!/bin/bash
# Refreshes everything in this folder for a pinned release, in one step:
#
#   1. Resolves the target tag's full commit SHA (or, with no argument, reads
#      whichever tag is already pinned in the manifest, e.g. after Flathub's
#      external-data-checker bot bumped it).
#   2. Checks that tag out into a disposable git worktree (so the current
#      working tree, and whatever branch it is on, is left untouched).
#   3. Runs ../generate-nuget-sources.sh against THAT commit's UI.csproj, so
#      nuget-sources.json matches the exact dependency graph of the pinned
#      release rather than whatever HEAD happens to be.
#   4. Copies the regenerated nuget-sources.json here and, only when a tag
#      argument was given, updates the tag:/commit: fields in
#      dk.nikse.subtitleedit.yaml to match.
#
# This is the one command to run after every new stable release, per
# https://github.com/SubtitleEdit/subtitleedit/issues/11800 (niksedk asked
# for the per-release manifest refresh to not be a lot of manual work). CI
# calls it with no argument in .github/workflows/build-ui.yml
# (regenerate-flathub-nuget-sources), so nuget-sources.json is regenerated
# and the git-source manifest is build-tested on every run, not just when
# someone remembers to do it locally.
#
# What this script does NOT do: bump the metainfo <release> entry (run
# ../update-metainfo-version.sh separately, same as the CI manifest), submit
# anything to flathub/flathub, or push to flathub/dk.nikse.subtitleedit.
#
# Requires: git, and either (a) flatpak + org.freedesktop.Sdk.Extension.dotnet10//24.08,
# or (b) a system dotnet 10 SDK install (see ../generate-nuget-sources.sh for details
# on both paths).
#
# Usage (from repo root):
#   ./installer/flatpak/flathub/prepare-flathub-release.sh          # regenerate for the current pin
#   ./installer/flatpak/flathub/prepare-flathub-release.sh v5.1.0   # bump the pin to v5.1.0, then regenerate

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
MANIFEST="$SCRIPT_DIR/dk.nikse.subtitleedit.yaml"
NUGET_SOURCES="$SCRIPT_DIR/nuget-sources.json"

cd "$REPO_ROOT"

if [ -n "${1:-}" ]; then
    TAG="$1"
    echo "Resolving commit for $TAG..."
    git fetch --tags --quiet origin "$TAG" 2>/dev/null || true
    COMMIT="$(git rev-parse "$TAG^{commit}")"
    UPDATE_PIN=1
else
    echo "No tag given, reading the currently pinned tag/commit from the manifest..."
    read -r TAG COMMIT < <(python3 - "$MANIFEST" << 'PYTHON'
import re
import sys

path = sys.argv[1]
lines = open(path, encoding="utf-8").readlines()

anchor = next(
    (i for i, line in enumerate(lines)
     if "url: https://github.com/SubtitleEdit/subtitleedit.git" in line),
    None,
)
if anchor is None:
    sys.exit("ERROR: could not find the SubtitleEdit git source anchor line in the manifest")

tag_idx, commit_idx = anchor + 1, anchor + 2
tag_match = re.search(r"tag:\s*(\S+)", lines[tag_idx])
commit_match = re.search(r"commit:\s*(\S+)", lines[commit_idx])
if not tag_match or not commit_match:
    sys.exit("ERROR: unexpected manifest layout near the SubtitleEdit source")

print(f"{tag_match.group(1)} {commit_match.group(1)}")
PYTHON
    )
    UPDATE_PIN=0
fi
echo "  $TAG -> $COMMIT"

WORKTREE="$(mktemp -d)"
trap 'git worktree remove --force "$WORKTREE" 2>/dev/null; rm -rf "$WORKTREE"' EXIT

# A shallow checkout (as CI uses) won't have the pinned commit's objects unless
# fetched explicitly. Harmless (and a no-op) on a full local clone that already has it.
git fetch --quiet origin "$COMMIT" 2>/dev/null || true

echo "Checking out $TAG into a disposable worktree..."
git worktree add --quiet --detach "$WORKTREE" "$COMMIT"

echo "Regenerating nuget-sources.json for $TAG's UI.csproj..."
(
    cd "$WORKTREE"
    ./installer/flatpak/generate-nuget-sources.sh
)

cp "$WORKTREE/installer/flatpak/nuget-sources.json" "$NUGET_SOURCES"
echo "  Copied to $NUGET_SOURCES"

if [ "$UPDATE_PIN" = "1" ]; then
    echo "Updating tag/commit pin in $(basename "$MANIFEST")..."
    # The manifest also pins tag:/commit: for unrelated dependency modules (harfbuzz,
    # libass, ffmpeg, mpv, x264, ...), so a blind sed across the whole file would
    # re-pin those too. Only touch the two lines directly following the anchor line
    # that names this app's own git source.
    python3 - "$MANIFEST" "$TAG" "$COMMIT" << 'PYTHON'
import re
import sys

path, new_tag, new_commit = sys.argv[1], sys.argv[2], sys.argv[3]
lines = open(path, encoding="utf-8").readlines()

anchor = next(
    (i for i, line in enumerate(lines)
     if "url: https://github.com/SubtitleEdit/subtitleedit.git" in line),
    None,
)
if anchor is None:
    sys.exit("ERROR: could not find the SubtitleEdit git source anchor line, refusing to edit blindly")

tag_idx, commit_idx = anchor + 1, anchor + 2
if "tag:" not in lines[tag_idx] or "commit:" not in lines[commit_idx]:
    sys.exit("ERROR: unexpected manifest layout near the SubtitleEdit source, refusing to edit blindly")

lines[tag_idx] = re.sub(r"tag:.*", f"tag: {new_tag}", lines[tag_idx])
lines[commit_idx] = re.sub(r"commit:.*", f"commit: {new_commit}", lines[commit_idx])

open(path, "w", encoding="utf-8").writelines(lines)
print(f"  tag -> {new_tag}, commit -> {new_commit}")
PYTHON
else
    echo "No tag argument given, leaving the manifest's pin as-is (just refreshed nuget-sources.json for it)."
fi

echo ""
echo "Done. Review the diff, then validate before opening a PR:"
echo "  git -C \"$REPO_ROOT\" diff -- installer/flatpak/flathub/"
echo "  appstreamcli validate installer/flatpak/dk.nikse.subtitleedit.metainfo.xml"
echo "  flatpak run --command=flatpak-builder-lint org.flatpak.Builder manifest \\"
echo "    installer/flatpak/flathub/dk.nikse.subtitleedit.yaml"
