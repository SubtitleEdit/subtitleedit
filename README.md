# Subtitle Edit

Subtitle Edit is a free, open-source subtitle editor for creating, editing, synchronizing, translating, running OCR on, and converting subtitles in 300+ formats.

![Subtitle Edit main window](docs/screenshots/main-window.png)

## Documentation

- [Documentation and FAQ](http://subtitleedit.github.io/subtitleedit/)
- [Feature overview](docs/overview.md)
- [Command-line converter reference](docs/reference/command-line.md)
- [Issue tracker](https://github.com/SubtitleEdit/subtitleedit/issues)

## Downloads

Ready-to-run packages are available on the [Releases](https://github.com/SubtitleEdit/subtitleedit/releases) page.

Available release packages include:

- Windows x64 installer
- Windows x64 and ARM64 zip packages
- macOS x64 and ARM64 dmg packages
- Linux x64 tarball
- Linux x64 Flatpak bundle

Provided release builds are self-contained and do not require a separate .NET installation.

## Key Features

- Create, edit, synchronize, and convert subtitles in 300+ formats
- Preview video and audio with waveform and spectrogram support
- Speech-to-text workflows with local Whisper-based engines and downloadable models
- OCR for image-based subtitles such as Blu-ray SUP and VobSub
- Automatic translation, text-to-speech, spell check, and common-error fixing
- Batch conversion and a separate command-line converter for automation
- ASSA/SSA styling, positioning, drawing, and advanced override tags
- Local auto-backup and offline-first editing for normal subtitle workflows

## System Requirements

### Windows

- Windows 10 or newer

### macOS

- macOS 12 Monterey or newer
- Intel Macs need `mpv` and `ffmpeg`, for example via [MacPorts](https://www.macports.org/):

```bash
sudo port install mpv ffmpeg
```

Because Subtitle Edit for macOS is not signed with an Apple developer certificate, macOS can block it on first launch. After copying `Subtitle Edit.app` to `Applications`, you can remove the quarantine flag and add an ad-hoc signature:

```bash
sudo xattr -rd com.apple.quarantine "/Applications/Subtitle Edit.app"
sudo codesign --force --deep --sign - "/Applications/Subtitle Edit.app"
```

### Linux

The Flatpak bundle from the [Releases](https://github.com/SubtitleEdit/subtitleedit/releases) page includes the required media dependencies:

```bash
flatpak install SubtitleEdit-linux-x64.flatpak
flatpak run dk.nikse.subtitleedit
```

Native package users should install `mpv` and `ffmpeg` for video playback and audio/video processing:

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

## Building from Source

Subtitle Edit 5 targets .NET 10 and uses Avalonia UI for the cross-platform desktop application. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) before building from source.

```bash
dotnet restore SubtitleEdit.sln
dotnet msbuild SubtitleEdit.sln /p:Configuration=Release
```

After a successful Windows build, the desktop application is available at:

```text
src/ui/bin/Release/net10.0/SubtitleEdit.exe
```

## Privacy

Subtitle Edit is an offline, open-source application. It does not collect, store, transmit, or analyze the content of your subtitle files, media files, or associated metadata for analytics, model training, or any other secondary purpose.

Core features such as editing, converting, video playback, and local auto-backup run on your device. If you choose optional third-party online services for translation, speech-to-text, text-to-speech, OCR, or lookups, only the data required for that request is sent directly to the selected provider and governed by that provider's privacy policy.

## Support the Project

If you would like to support continued development of Subtitle Edit, please consider donating:

- [GitHub Sponsors](https://github.com/sponsors/niksedk)
- [Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU)

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=SubtitleEdit%2Fsubtitleedit&type=Date&legend=top-left)](https://www.star-history.com/?repos=SubtitleEdit%2Fsubtitleedit&type=date&legend=top-left)
