# Flathub submission

This folder holds the **Flathub-ready** variant of the Flatpak manifest. It differs from the
CI manifest (`../dk.nikse.subtitleedit.yaml`, which builds from a local `type: dir` checkout)
only in the app source: Flathub builds from the public repository at a pinned release tag +
commit.

Everything else (bundled mpv/ffmpeg/tesseract+eng, offline NuGet restore, desktop/metainfo/icon/
license install) is identical and already validated in CI by the `regenerate-flatpak-nuget-sources` /
`build-linux-flatpak` jobs in `.github/workflows/build-ui.yml`.

## Files
- `dk.nikse.subtitleedit.yaml`: the manifest to submit (git source, pinned to a tag + commit).
- `nuget-sources.json`: offline NuGet packages for that exact pinned commit.
- `prepare-flathub-release.sh`: regenerates both of the above for a new tag in one step.

The `.desktop`, `.metainfo.xml` and icon are installed from the git checkout during the build, so
they do not need to be copied here.

## CI validates and regenerates this automatically

`.github/workflows/build-ui.yml` has a `regenerate-flathub-nuget-sources` → `build-linux-flathub`
job pair, the same pattern already used for the CI manifest's own
`regenerate-flatpak-nuget-sources` → `build-linux-flatpak` pair. On every dispatch it:

1. Reads whatever tag/commit is currently pinned in `dk.nikse.subtitleedit.yaml`, checks that
   commit out directly (this job's own workspace is disposable, so a detached checkout is
   safe), and regenerates `nuget-sources.json` for it, uploaded as the `flathub-nuget-sources`
   artifact. This checks out in place rather than using `prepare-flathub-release.sh`'s
   git-worktree approach: that approach is fine for local/manual use, but the preferred NuGet
   restore path shells out to a second, nested Flatpak sandbox, which failed to resolve the
   worktree's path when this job already runs inside its own container. Checking out directly
   sidesteps that instead of chasing it further.
2. Builds the actual git-source manifest, the one that would be submitted to Flathub, in the
   same `flathub-infra` container Flathub itself uses, for both `x86_64` and `aarch64`
   (native ARM64 runner, not emulation), and uploads each as `flathub-bundle-x86_64` /
   `flathub-bundle-aarch64`.

That last part is the real point: it validates this exact variant end to end on a real Linux
runner, without needing a contributor's own Linux box or local Flatpak install (see the
discussion on #11800). The two architectures matter because a contributor testing this ahead
of submission may well be on aarch64 hardware (e.g. Apple Silicon under a Linux VM); an
x86_64-only bundle would force them into qemu emulation just to try it. Run the workflow,
download whichever `flathub-bundle-<arch>` matches your machine, and if it built, the pin
is good; `flathub-nuget-sources` is the file to commit here (the same one covers both
architectures, since `generate-nuget-sources.sh` already restores for both).

`nuget-sources.json` is intentionally not committed to this repo until it has been generated
this way, so that it can never silently drift out of sync with whatever the manifest currently
pins (the CI job would recreate it fresh from the pin every time it runs).

## Keeping this current for each new release

Two mechanisms, doing two different halves of the job:

1. **Tag/commit bump**: the manifest's `x-checker-data` block lets Flathub's own
   [external-data-checker bot](https://github.com/flathub-infra/flatpak-external-data-checker)
   watch this repo's tags and open a PR against `flathub/dk.nikse.subtitleedit` bumping
   `tag:`/`commit:` automatically when a new stable tag (matching `v<major>.<minor>.<patch>`,
   so pre-releases like `-beta*` are skipped) is pushed. This part is hands-off once the app
   is accepted.

2. **nuget-sources.json regeneration**: the bot has no way to know how to run our custom NuGet
   pre-fetch step, so it does not touch `nuget-sources.json`. After merging one of its PRs (or
   before any manual release), either dispatch `build-ui.yml` and pull the refreshed file from
   the `flathub-nuget-sources` artifact, or run locally:
   ```sh
   ./installer/flatpak/flathub/prepare-flathub-release.sh v5.1.0
   ```
   from the repo root (only needed if bumping to a new tag outside of what the checker bot
   already wrote into the manifest; with no argument it just regenerates for whatever is
   currently pinned). This resolves the tag's commit, regenerates `nuget-sources.json` for
   that exact commit in a disposable worktree (so it does not disturb whatever branch you are
   on), and updates the manifest's `tag:`/`commit:` to match. That is the entire per-release
   workflow; nothing else in this folder needs manual editing.

   Also run `../update-metainfo-version.sh` (bumps the AppStream `<release>` entry) and add a
   matching entry to `../dk.nikse.subtitleedit.metainfo.xml` if this is a new stable version,
   same as before.

## Before submitting (first-time only)
1. **Validate** (the Flathub review bot also runs this, but catching problems before opening
   the PR saves review rounds). Can be run as a one-off step inside the same CI container
   used above, or locally on Linux:
   ```sh
   appstreamcli validate installer/flatpak/dk.nikse.subtitleedit.metainfo.xml
   flatpak run --command=flatpak-builder-lint org.flatpak.Builder manifest \
     installer/flatpak/flathub/dk.nikse.subtitleedit.yaml
   ```
2. **Actually run the built app**, not just build it. `build-linux-flathub` in CI proves the
   sandbox builds; it does not launch the app or exercise it. Download whichever
   `flathub-bundle-<arch>` matches your machine and install/run it on Linux
   (`flatpak install SubtitleEdit-flathub-<arch>.flatpak && flatpak run
   dk.nikse.subtitleedit`), open a subtitle file, load a video, try OCR, before submitting.

## Submitting
1. Fork `https://github.com/flathub/flathub`.
2. Create a branch **named exactly `dk.nikse.subtitleedit`**.
3. Add `dk.nikse.subtitleedit.yaml` and `nuget-sources.json` at the repo root of that branch.
4. Open a PR against the **`new-pr`** branch of `flathub/flathub`. A bot builds it; reviewers review.
5. On acceptance, Flathub creates the `flathub/dk.nikse.subtitleedit` repo (you maintain it there).
   Push updates there directly, or let the auto-update bot's PRs land after the
   `nuget-sources.json` fixup described above.
6. Verify the app on flathub.org (prove ownership of `nikse.dk` or the GitHub org).

## Likely review questions (be ready)
- **Runtime downloads of executables** (whisper.cpp, CrispASR, TTS models). Core editing, OCR
  (bundled Tesseract) and video (bundled mpv/ffmpeg) work fully offline; the optional AI engines
  fetch executables at runtime into the app's own data directory, which Flathub scrutinizes.
  These downloads are user-initiated and opt-in, and every artifact is checked against a known
  SHA-256 hash before use (`src/ui/Logic/Download/DownloadHashManager.cs`), so this is a good,
  concrete answer if reviewers ask about integrity, not just an "it's optional" hand-wave.
- **`--filesystem=home` + `--share=network`**: broad but standard for an editor with online
  services (translation, TTS, speech-to-text).
- **App-id `dk.nikse.subtitleedit`**: fine if `nikse.dk` is verified later on flathub.org;
  `io.github.SubtitleEdit.subtitleedit` is the easier-to-verify alternative (but renaming the
  app-id touches the desktop/metainfo/icon filenames and the settings directory).
