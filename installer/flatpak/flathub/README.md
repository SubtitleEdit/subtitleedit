# Flathub submission

This folder holds the **Flathub-ready** variant of the Flatpak manifest. It differs from the
CI manifest (`../dk.nikse.subtitleedit.yaml`, which builds from a local `type: dir` checkout)
only in the app source: Flathub builds from the public repository at a pinned release tag +
commit.

Everything else (bundled mpv/ffmpeg/tesseract+eng, offline NuGet restore, desktop/metainfo/icon/
license install) is identical and already validated in CI by `.github/workflows/build-flatpak.yml`.

## Files
- `dk.nikse.subtitleedit.yaml` — the manifest to submit (git source, pinned to a tag + commit).
- `nuget-sources.json` — offline NuGet packages. **Must be regenerated for the pinned commit.**

The `.desktop`, `.metainfo.xml` and icon are installed from the git checkout during the build, so
they do not need to be copied here.

## Before submitting
1. **Pin the release.** Set `tag:` and `commit:` in the manifest to the release you want to ship
   (currently `v5.0.0` / `a44ffa7025b59f424aa8a5a7ee250ea85c4c1361`).
2. **Regenerate `nuget-sources.json`** for that exact commit:
   ```sh
   git checkout v5.0.0
   installer/flatpak/generate-nuget-sources.sh
   cp installer/flatpak/nuget-sources.json installer/flatpak/flathub/nuget-sources.json
   ```
3. **Validate** (on Linux, or rely on the Flathub bot):
   ```sh
   appstreamcli validate installer/flatpak/dk.nikse.subtitleedit.metainfo.xml
   flatpak run --command=flatpak-builder-lint org.flatpak.Builder manifest \
     installer/flatpak/flathub/dk.nikse.subtitleedit.yaml
   ```

## Submitting
1. Fork `https://github.com/flathub/flathub`.
2. Create a branch **named exactly `dk.nikse.subtitleedit`**.
3. Add `dk.nikse.subtitleedit.yaml` and `nuget-sources.json` at the repo root of that branch.
4. Open a PR against the **`new-pr`** branch of `flathub/flathub`. A bot builds it; reviewers review.
5. On acceptance, Flathub creates the `flathub/dk.nikse.subtitleedit` repo (you maintain it there).
   Push updates / let the auto-update bot PR on new GitHub releases.
6. Verify the app on flathub.org (prove ownership of `nikse.dk` or the GitHub org).

## Likely review questions (be ready)
- **Runtime downloads of executables** (whisper.cpp, CrispASR, TTS/STT). Core editing, OCR (bundled
  Tesseract) and video (bundled mpv/ffmpeg) work offline; the optional AI engines fetch executables
  at runtime, which Flathub scrutinizes. Justify as optional + user-initiated into the app data dir,
  or expect to limit/disable those under the sandbox.
- **`--filesystem=home` + `--share=network`** — broad but standard for an editor with online services.
- **App-id `dk.nikse.subtitleedit`** — fine if `nikse.dk` is verified later on flathub.org;
  `io.github.SubtitleEdit.subtitleedit` is the easier-to-verify alternative (but renaming the app-id
  touches the desktop/metainfo/icon filenames and the settings directory).
