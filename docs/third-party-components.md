---
layout: default
title: Third-Party Components
---

# Third-Party Components

Subtitle Edit uses several third-party tools for features like video playback, audio extraction, and OCR. While Subtitle Edit includes built-in downloaders for these components, you might want to use a specific version or a custom build.

> **⚠️ Warning**
> Subtitle Edit is tested with specific versions of these components. Using other versions is **not officially supported** and may cause instability.
>

## Where are the files located?

Subtitle Edit stores these components in its **Data Folder**.

*   **Portable Version:** The folder containing `SubtitleEdit.exe` (Windows) or the executable.
*   **Installed Version (Windows):** `%APPDATA%\Subtitle Edit`
    *   (Press `Win+R`, type `%APPDATA%\Subtitle Edit`, and hit Enter)
*   **Linux:** `~/.config/Subtitle Edit` (or `$XDG_CONFIG_HOME/Subtitle Edit`).
*   **macOS:** `~/Library/Application Support/Subtitle Edit`.

> **Tip:** You can open the Data Folder directly from Subtitle Edit by pressing `Ctrl+Alt+Shift+D` (Windows/Linux) or `Cmd+Alt+Shift+D` (macOS).

---

## Windows

### Quick Reference Table

| Component | File(s) | Destination Path |
|-----------|---------|------------------|
| **FFmpeg** | `ffmpeg.exe`, `ffprobe.exe` (optional) | `[Data Folder]/ffmpeg` |
| **MPV** | `libmpv-2.dll` | `[Data Folder]` (root) |
| **Tesseract** | `tesseract.exe`, `tessdata/` folder | `[Data Folder]/Tesseract550` |
| **Whisper CPP** | `whisper-cli.exe`, `Models/` folder | `[Data Folder]/Whisper/Cpp` |
| **Purfview Faster-Whisper** | `faster-whisper-xxl.exe`, `_models/` folder | `[Data Folder]/Whisper/Purfview-Whisper-Faster` |

---

### FFmpeg
Used for reading media info, extracting audio, and generating waveforms.

*   **Download:** [ffmpeg.org](https://ffmpeg.org/download.html) — Look for builds from `gyan.dev` or `BtbN`. Use "release-essentials" or "release-full".
*   **Destination:** `[Data Folder]/ffmpeg`
*   **Files to place:**
    *   Extract `ffmpeg.exe` from the download (usually found in a `bin` subfolder).
    *   Place `ffmpeg.exe` **directly** in `[Data Folder]/ffmpeg` — **do not** include the `bin` subfolder itself.
    *   (Optional) `ffprobe.exe` can also be placed in the same folder.

### MPV Media Player (libmpv)
Used as a video player engine.

*   **Download:** [mpv-winbuild-cmake Releases](https://github.com/shinchiro/mpv-winbuild-cmake/releases)
    *   Look for files starting with **`mpv-dev-...`** (e.g., `mpv-dev-x86_64-20260226-git-d54bad5.7z`).
    *   **Note:** Builds with "v3" in the filename (e.g., `mpv-dev-x86_64-v3-...`) may offer better performance but require a newer CPU with AVX2 support. Use the standard builds (without "v3") for broader compatibility.
*   **Destination:** `[Data Folder]` (The root data folder)
*   **Files:** Extract `libmpv-2.dll` to the **root** of the Data Folder.

### Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

*   **Download:** [UB-Mannheim Tesseract](https://github.com/UB-Mannheim/tesseract/wiki)
*   **Destination:** `[Data Folder]/Tesseract550`
*   **Files:** The content of the installation folder (containing `tesseract.exe` and `tessdata` folder) should be placed here.

### Whisper CPP (Speech-to-Text)
Used for AI-based speech recognition.

*   **Download:** [ggerganov/whisper.cpp releases](https://github.com/ggerganov/whisper.cpp/releases)
*   **Destination:** `[Data Folder]/Whisper/Cpp`
*   **Files:** Download the Windows zip and extract to the destination folder. The download already includes `whisper-cli.exe`.
*   **Models:** Models (`.bin` files) go into a `Models` subfolder: `[Data Folder]/Whisper/Cpp/Models`.

> **Note:** It is generally recommended to use the internal downloader for Whisper due to the complexity of model and library dependencies.

### Purfview Faster-Whisper (GPU Speech-to-Text)
Used for GPU-accelerated AI-based speech recognition.

*   **Download:** [Purfview/whisper-standalone-win releases](https://github.com/Purfview/whisper-standalone-win/releases)
*   **Destination:** `[Data Folder]/Whisper/Purfview-Whisper-Faster`
*   **Files:** Download the Standalone Archive, extract contents so `faster-whisper-xxl.exe` is in the folder root.
*   **Models:** Place model directories (e.g., `faster-whisper-medium`) inside the `_models` folder.

---

## Linux

### FFmpeg
Used for reading media info, extracting audio, and generating waveforms.

*   **Install:** Via package manager (e.g., `sudo apt install ffmpeg`) or download static builds from [ffmpeg.org](https://ffmpeg.org/download.html).
*   **Files:** Subtitle Edit will look for `ffmpeg` in system paths (e.g., `/usr/bin/ffmpeg`).
*   **Alternative:** Place the `ffmpeg` binary in `[Data Folder]/ffmpeg`.

### MPV Media Player (libmpv)
Used as a video player engine.

*   **Install:** Use your package manager to install `libmpv` (e.g., `sudo apt install libmpv2` or `libmpv-dev`).
*   **Files:** Subtitle Edit looks for `libmpv.so.2` or `libmpv.so` in standard library paths (`/usr/lib`, `/usr/local/lib`, etc.).

### Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

*   **Install:** Use package manager (e.g., `sudo apt install tesseract-ocr`).
*   **Files:** Subtitle Edit will detect the system installation. Ensure language data (`tessdata`) is also installed (often separate packages).

### Whisper CPP (Speech-to-Text)
Used for AI-based speech recognition.

*   **Download:** [ggerganov/whisper.cpp releases](https://github.com/ggerganov/whisper.cpp/releases) or build from source.
*   **Destination:** `[Data Folder]/Whisper/Cpp`
*   **Files:** Download or build the binary and ensure it is named `whisper-cli`.
*   **Models:** Models (`.bin` files) go into a `Models` subfolder: `[Data Folder]/Whisper/Cpp/Models`.

### Purfview Faster-Whisper (GPU Speech-to-Text)
Used for GPU-accelerated AI-based speech recognition.

*   **Download:** [Purfview/whisper-standalone-win releases](https://github.com/Purfview/whisper-standalone-win/releases)
*   **Destination:** `[Data Folder]/Whisper/Purfview-Whisper-Faster`
*   **Files:** Download the Linux Archive, extract so `faster-whisper-xxl` binary is present.
*   **Models:** Place model directories (e.g., `faster-whisper-medium`) inside the `_models` folder.

---

## macOS

### FFmpeg
Used for reading media info, extracting audio, and generating waveforms.

*   **Install:** Via Homebrew (e.g., `brew install ffmpeg`) or download static builds from [ffmpeg.org](https://ffmpeg.org/download.html).
*   **Files:** Subtitle Edit will look for `ffmpeg` in system paths (e.g., `/opt/homebrew/bin/ffmpeg`).
*   **Alternative:** Place the `ffmpeg` binary in `[Data Folder]/ffmpeg`.

### MPV Media Player (libmpv)
Used as a video player engine.

*   **Install:** Use Homebrew (e.g., `brew install mpv`).
*   **Files:** Subtitle Edit looks for `libmpv.dylib` or `libmpv.2.dylib` in standard library paths (`/opt/homebrew/lib`, `/usr/local/lib`, etc.).

### Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

*   **Install:** Use Homebrew (e.g., `brew install tesseract`).
*   **Files:** Subtitle Edit will detect the system installation. Ensure language data (`tessdata`) is also installed.

### Whisper CPP (Speech-to-Text)
Used for AI-based speech recognition.

*   **Download:** [ggerganov/whisper.cpp releases](https://github.com/ggerganov/whisper.cpp/releases) or build from source.
*   **Destination:** `[Data Folder]/Whisper/Cpp`
*   **Files:** Download or build the binary and ensure it is named `whisper-cli`.
*   **Models:** Models (`.bin` files) go into a `Models` subfolder: `[Data Folder]/Whisper/Cpp/Models`.
