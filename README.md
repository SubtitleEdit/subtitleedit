# Subtitle Edit

The subtitle editor :)

---

## 🌐 Documentation & FAQ
http://subtitleedit.github.io/subtitleedit/

---

## 🚀 Automated Builds
You can find the latest cross-platform builds here:  
👉 [Releases](https://github.com/SubtitleEdit/subtitleedit/releases)

---

## 💻 System Requirements

### Windows
- Minimum: Windows 10 or newer

### macOS

- **Minimum macOS version**: 12 (Monterey) or newer
- **Dependencies for Intel macs** (install via [MacPorts](https://www.macports.org/)):
    - `mpv`
        - MacPorts: `sudo port install mpv`
    - `ffmpeg`
        - Homebrew: `sudo port install ffmpeg`



#### Installing Subtitle Edit on macOS (Unsigned App)

Because *Subtitle Edit* is not signed with an Apple developer certificate, macOS will block it by default. You can still install and run it by following these steps:

1. **Download** and **double-click** the `.dmg` file to mount it.
2. In the window that appears, **drag `Subtitle Edit.app` into your `Applications` folder**.
3. Open the **Terminal** app (you can find it via Spotlight or in `/Applications/Utilities/`).
4. In Terminal, run the following commands to remove macOS’s security quarantine flag and add adhoc code signature:
   ````bash
   sudo xattr -rd com.apple.quarantine "/Applications/Subtitle Edit.app"
   ````

   ````bash
   sudo codesign --force --deep --sign - "/Applications/Subtitle Edit.app"
   ````

### Linux
Requires mpv and ffmpeg (ffmpeg is normally already installed) to enable video functionality.

#### Debian/Ubuntu
```bash
sudo apt update && sudo apt install -y mpv libmpv-dev ffmpeg
```

#### Arch
```bash
sudo pacman -S mpv ffmpeg
```

#### Fedora
```bash
sudo dnf install mpv-libs ffmpeg
```

#### openSUSE
```bash
sudo zypper install libmpv1 ffmpeg
```

> ⚙️ Note: The provided builds are self-contained and do not require a separate .NET installation.

---

## 🔒 Privacy

**Subtitle Edit** is an offline, open-source application.  
It does **not** collect, store, transmit, or analyze the content of your subtitle files, media files, or any associated metadata — not for analytics, not for model training, and not for any other secondary purpose, now or in the future.

All core features, including editing, converting, video playback, and **local auto-backup**, run entirely on your device.

If you choose to use optional third-party online services within Subtitle Edit (such as translation, speech-to-text, text-to-speech, OCR, or dictionary/lookups), only the minimal data required to perform that specific request is sent directly to the selected provider. Any such data transfer is governed by the provider’s own privacy policy, and Subtitle Edit does not retain or forward this data in any way.

Subtitle Edit aims to give you full control over your files — your data stays yours.

---

## ❤️ Support the Project
If you’d like to support the continued development of Subtitle Edit, please consider donating:

- [GitHub Sponsors](https://github.com/sponsors/niksedk)
- [Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=4XEHVLANCQBCU)

---
