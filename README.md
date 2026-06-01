# Subtitle Edit

<p align="center">
  <img src=".github/assets/subtitle-edit-logo.jpg" alt="Subtitle Edit logo" width="420">
</p>

<p align="center">
  <strong>A cross-platform subtitle editor for creating, correcting, converting, syncing, and translating subtitles.</strong>
</p>

<p align="center">
  <a href="https://github.com/SubtitleEdit/subtitleedit/releases">Releases</a> ·
  <a href="http://subtitleedit.github.io/subtitleedit/">Documentation</a> ·
  <a href="https://github.com/SubtitleEdit/subtitleedit/issues">Issues</a> ·
  <a href="https://github.com/sponsors/niksedk">Sponsor</a>
</p>

---

## ✨ What is Subtitle Edit?

Subtitle Edit is an offline, open-source subtitle editor for Windows, macOS, and Linux. It helps you edit, convert, synchronize, translate, OCR, and quality-check subtitle files with video preview support.

## ⭐ Highlights

- Edit and synchronize subtitles with video/audio waveform support.
- Convert between many subtitle formats, including SRT, ASS/SSA, VTT, TTML/DFXP, STL, and more.
- Fix common subtitle issues, spell-check text, and clean formatting.
- Use OCR, speech-to-text, translation, and text-to-speech workflows when configured.
- Run offline for core editing features; optional online services are only used when you explicitly configure them.

## 🌐 Documentation & FAQ

Documentation, guides, and frequently asked questions are available here:

👉 http://subtitleedit.github.io/subtitleedit/

## 🚀 Downloads

You can find the latest builds on the Releases page:

👉 [Releases](https://github.com/SubtitleEdit/subtitleedit/releases)

## 💻 System requirements

### Windows

- Windows 10 version 22H2 (build 19045) or newer, fully updated.
- Older Windows 10 builds (2004/20H2/21H1/21H2) are end-of-life and may fail to start with a .NET runtime error (`0x80131506`).

### macOS

- macOS 12 Monterey or newer.
- The `.dmg` build is self-contained: `libmpv` and `ffmpeg` are bundled inside `Subtitle Edit.app`, so MacPorts or Homebrew are not required.

#### Installing Subtitle Edit on macOS unsigned builds

Because Subtitle Edit is not signed with an Apple developer certificate, macOS may block it by default. You can still install and run it with these steps:

1. Download and double-click the `.dmg` file to mount it.
2. Drag `Subtitle Edit.app` into your `Applications` folder.
3. Open Terminal from Spotlight or `/Applications/Utilities/`.
4. Remove the quarantine flag and add an ad-hoc code signature:

```bash
sudo xattr -rd com.apple.quarantine "/Applications/Subtitle Edit.app"
sudo codesign --force --deep --sign - "/Applications/Subtitle Edit.app"
```

### Linux

#### Flatpak

A Flatpak package is available from the Releases page. It bundles the required dependencies, including mpv and ffmpeg.

```bash
flatpak install SubtitleEdit-linux-x64.flatpak
flatpak run dk.nikse.subtitleedit
```

#### Native packages

Video functionality requires mpv and ffmpeg. ffmpeg is often already installed.

```bash
# Debian/Ubuntu
sudo apt update && sudo apt install -y mpv libmpv-dev ffmpeg

# Arch
sudo pacman -S mpv ffmpeg

# Fedora
sudo dnf install mpv-libs ffmpeg

# openSUSE
sudo zypper install libmpv1 ffmpeg
```

> Note: The provided builds are self-contained and do not require a separate .NET installation.

## 🔒 Privacy

Subtitle Edit is an offline, open-source application by default. It does not collect, store, transmit, or analyze the content of your subtitle files, media files, or associated metadata for analytics, model training, or other secondary purposes.

Core features such as editing, converting, video playback, and local auto-backup run on your device.

Optional third-party online services, such as translation, speech-to-text, text-to-speech, OCR, or dictionary lookups, may send the minimum data required for the action you explicitly request. Those transfers are governed by the selected provider’s own privacy policy. Subtitle Edit does not retain or forward that data beyond the configured provider workflow.

## ❤️ Support the project

If you would like to support the continued development of Subtitle Edit, please consider donating:

- [GitHub Sponsors](https://github.com/sponsors/niksedk)
- [Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU)

## 📄 License

Subtitle Edit is released under the MIT License. See [LICENSE](LICENSE) for details.
