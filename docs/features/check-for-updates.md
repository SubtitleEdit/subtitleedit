# Check for Updates

See whether a newer Subtitle Edit is available and, for most installs, update without leaving the app.

**Menu:** `Help` → `Check for updates...`

## How to Use

1. Go to **Help** → **Check for updates...**.
2. Subtitle Edit asks GitHub for the latest release and shows the change log.
3. If a newer version exists you get one or two buttons:
   - **Install and restart** — Downloads the update, checks it, installs it and starts the new version (see [which installs support this](#which-installs-can-update-in-the-app))
   - **Download new version** — Opens the GitHub releases page so you can download it yourself

When you click **Install and restart**, Subtitle Edit downloads the file with a progress bar and then closes. If you have unsaved changes you are asked about them first. The update is installed once Subtitle Edit has closed, and the new version starts. If you choose to keep working instead of closing, the update is installed the next time you quit.

Each download is checked against the SHA-256 checksum GitHub publishes for the release file. If anything goes wrong, the dialog says *Automatic update failed* and you can use **Download new version** instead. The details go to `error-log.txt` in the Subtitle Edit data folder.

## Which installs can update in the app

| Install | In-app update |
|---------|---------------|
| Windows x64 installer (`SubtitleEdit-Windows-x64-Setup.exe`) | Yes |
| Windows portable zip | No: download page |
| Windows ARM64 (zip only) | No: download page |
| macOS app (Intel or Apple silicon) | Yes, if the app is in a folder you can write to (such as Applications) and is not run straight from the dmg |
| Linux tarball (x64 or ARM64) | Yes, if it is unpacked into a folder you can write to |
| Flatpak, AUR, AppImage and other packages | No: update with your package manager or store |

When in-app update is not supported, the dialog shows only **Download new version**.

The **Install and restart** button only appears when a version newer than the one you are running has been released.

## Stable and beta versions

Subtitle Edit has two release channels:

- **Stable versions only** — Recommended for most users
- **Stable and beta versions** — Also offers beta and release-candidate builds, for users who want to try new features early

If you have never picked a channel, stable builds look for stable updates only and beta builds also look for betas. To change it, open **Options** → **Settings** → **Updates** → **Notify about**, or use the gear button in the Check for updates dialog.

**Check for updates when the app starts** (same settings section) checks quietly shortly after start-up. If a new version is available, a download icon and "New version available" appear in the status bar, and clicking it opens the Check for updates dialog. It is hidden for Flatpak installs, which update through the store.

## Related

- [Settings](settings.md#updates)
