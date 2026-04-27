---
layout: default
title: Third-Party Components
---

# Third-Party Components

Subtitle Edit uses several third-party tools for features like video playback, audio extraction, and OCR. While Subtitle Edit includes built-in downloaders for these components, you might want to use a specific version or a custom build.

Subtitle Edit 5 also includes more downloadable AI components for speech-to-text, text-to-speech, and OCR. Prefer the in-app download prompts unless you need to install a specific build manually.

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
| **yt-dlp** | `yt-dlp.exe` | `[Data Folder]` (root) |
| **Tesseract** | `tesseract.exe`, `tessdata/` folder | `[Data Folder]/Tesseract550` |
| **Whisper CPP** | `whisper-cli.exe`, `Models/` folder | `[Data Folder]/SpeechToText/Cpp` |
| **Purfview Faster-Whisper XXL** | `faster-whisper-xxl.exe`, `_models/` folder | `[Data Folder]/SpeechToText/Purfview-Faster-Whisper-XXL` |
| **Crisp ASR** | `crispasr.exe`, `models/` folder | `[Data Folder]/SpeechToText/CrispASR` |
| **Qwen3 ASR CPP** | `qwen3-asr-cli.exe`, `models/` folder | `[Data Folder]/Qwen3ASR` |
| **Parakeet.cpp** | `parakeet.exe`, model folders | `[Data Folder]/parakeet.cpp` |
| **PaddleOCR** | `paddleocr.exe`, `models/` folder | `[Data Folder]/OCR/PaddleOCR3-1` |
| **Qwen3 TTS** | `qwen3-tts-server.exe`, `models/`, `voices/` | `[Data Folder]/TextToSpeech/Qwen3TtsCpp` |
| **Kokoro TTS** | `kokoro-tts-server.exe`, `models/` | `[Data Folder]/TextToSpeech/KokoroTtsCpp` |

---

### FFmpeg
Used for reading media info, extracting audio, and generating waveforms.

*   **Download:** [ffmpeg.org](https://ffmpeg.org/download.html) — Look for builds from `gyan.dev` or `BtbN`. Use "release-essentials" or "release-full".
*   **Destination:** `[Data Folder]/ffmpeg`
*   **Files to place:**
    *   Extract `ffmpeg.exe` from the download (usually found in a `bin` subfolder).
    *   Place `ffmpeg.exe` **directly** in `[Data Folder]/ffmpeg` — **do not** include the `bin` subfolder itself.
    *   (Optional) `ffprobe.exe` can also be placed in the same folder.
*   **Alternative: Custom Path**
    *   You can point to an existing FFmpeg installation in `Settings.json` (located in the Data Folder):
    ```json
    "FfmpegPath": "C:\\path\\to\\your\\ffmpeg.exe"
    ```
    *   Use double backslashes (`\\`) for Windows paths in JSON.

### MPV Media Player (libmpv)
Used as a video player engine.

*   **Download:** [mpv-winbuild-cmake Releases](https://github.com/shinchiro/mpv-winbuild-cmake/releases)
    *   Look for files starting with **`mpv-dev-...`** (e.g., `mpv-dev-x86_64-20260226-git-d54bad5.7z`).
    *   **Note:** Builds with "v3" in the filename (e.g., `mpv-dev-x86_64-v3-...`) may offer better performance but require a newer CPU with AVX2 support. Use the standard builds (without "v3") for broader compatibility.
*   **Destination:** `[Data Folder]` (The root data folder)
*   **Files:** Extract `libmpv-2.dll` to the **root** of the Data Folder.

### yt-dlp (Online Video Playback)
Used to enable mpv to stream online videos (e.g., YouTube, Vimeo, and [many other sites](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md)) via **Video > Open from URL**.

*   **Download:** [yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)
*   **Destination:** `[Data Folder]` (The root data folder)
*   **File:** Download `yt-dlp.exe` and place it directly in the root of the Data Folder.

> **Tip:** Subtitle Edit can download yt-dlp automatically. When you use **Video > Open from URL** for the first time, you will be prompted to download it.

### Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

*   **Download:** [UB-Mannheim Tesseract](https://github.com/UB-Mannheim/tesseract/wiki)
*   **Destination:** `[Data Folder]/Tesseract550`
*   **Files:** The content of the installation folder (containing `tesseract.exe` and `tessdata` folder) should be placed here.

### Whisper CPP (Speech-to-Text)
Used for AI-based speech recognition.

*   **Download:** [ggerganov/whisper.cpp releases](https://github.com/ggerganov/whisper.cpp/releases)
*   **Destination:** `[Data Folder]/SpeechToText/Cpp`
*   **Files:** Download the Windows zip and extract to the destination folder. The download already includes `whisper-cli.exe`.
*   **Models:** Models (`.bin` files) go into a `Models` subfolder: `[Data Folder]/SpeechToText/Cpp/Models`.

> **Note:** It is generally recommended to use the internal downloader for Whisper due to the complexity of model and library dependencies.

### Purfview Faster-Whisper (GPU Speech-to-Text)
Used for GPU-accelerated AI-based speech recognition.

*   **Download:** [Purfview/whisper-standalone-win releases](https://github.com/Purfview/whisper-standalone-win/releases)
*   **Destination:** `[Data Folder]/SpeechToText/Purfview-Faster-Whisper-XXL`
*   **Files:** Download the Standalone Archive, extract contents so `faster-whisper-xxl.exe` is in the folder root.
*   **Models:** Place model directories (e.g., `faster-whisper-medium`) inside the `_models` folder.

### SE5 Speech-to-Text Engines
Subtitle Edit 5 can download additional ASR engines directly from the **Speech to text** window.

*   **Crisp ASR:** Stored in `[Data Folder]/SpeechToText/CrispASR`. Models go into its `models` folder. Crisp ASR backends include Parakeet, Canary, Cohere, Fire Red, GLM, Granite, Qwen3, and Omni.
*   **Qwen3 ASR CPP:** Stored in `[Data Folder]/Qwen3ASR`. Models go into `[Data Folder]/Qwen3ASR/models`.
*   **Parakeet.cpp:** Stored in `[Data Folder]/parakeet.cpp`. Each model has its own folder because the model weights and `vocab.txt` must stay together.

Use [Speech to Text](features/speech-to-text.md) for the current engine list and workflow.

### PaddleOCR
Used for OCR of image-based subtitles.

*   **Destination:** `[Data Folder]/OCR/PaddleOCR3-1`
*   **Models:** `[Data Folder]/OCR/PaddleOCR3-1/models`
*   **Builds:** Subtitle Edit can download Windows CPU, Windows CUDA 11/12, Linux CPU, or Linux GPU builds when available.

### Local Text-to-Speech Engines
Subtitle Edit 5 can download local TTS servers and models from the **Text to speech** window.

*   **Qwen3 TTS:** Stored in `[Data Folder]/TextToSpeech/Qwen3TtsCpp`. Models go into the `models` folder; imported reference voices go into the `voices` folder.
*   **Kokoro TTS:** Stored in `[Data Folder]/TextToSpeech/KokoroTtsCpp`. Models go into the `models` folder.

Use [Text to Speech](features/text-to-speech.md) for engine-specific options.

---

## Linux

### FFmpeg
Used for reading media info, extracting audio, and generating waveforms.

*   **Install:** Via package manager (e.g., `sudo apt install ffmpeg`) or download static builds from [ffmpeg.org](https://ffmpeg.org/download.html).
*   **Files:** Subtitle Edit will look for `ffmpeg` in system paths (e.g., `/usr/bin/ffmpeg`).
*   **Alternative:** Place the `ffmpeg` binary in `[Data Folder]/ffmpeg`.
*   **Custom Path:** You can specify a custom path in `Settings.json` (located in the Data Folder):
    ```json
    "FfmpegPath": "/path/to/your/ffmpeg"
    ```

### MPV Media Player (libmpv)
Used as a video player engine.

*   **Install:** Use your package manager to install `libmpv` (e.g., `sudo apt install libmpv2` or `libmpv-dev`).
*   **Files:** Subtitle Edit looks for `libmpv.so.2` or `libmpv.so` in standard library paths (`/usr/lib`, `/usr/local/lib`, etc.).

### yt-dlp (Online Video Playback)
Used to enable mpv to stream online videos via **Video > Open from URL**.

*   **Download:** [yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)
*   **Destination:** `[Data Folder]` (The root data folder)
*   **File:** Download `yt-dlp_linux` and place it directly in the root of the Data Folder.
*   **Permissions:** The file must be executable. Run: `chmod +x yt-dlp_linux`

> **Tip:** Subtitle Edit can download yt-dlp automatically when you use **Video > Open from URL** for the first time.

### Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

*   **Install:** Use package manager (e.g., `sudo apt install tesseract-ocr`).
*   **Files:** Subtitle Edit will detect the system installation. Ensure language data (`tessdata`) is also installed (often separate packages).

### Whisper CPP (Speech-to-Text)
Used for AI-based speech recognition.

*   **Download:** [ggerganov/whisper.cpp releases](https://github.com/ggerganov/whisper.cpp/releases) or build from source.
*   **Destination:** `[Data Folder]/SpeechToText/Cpp`
*   **Files:** Download or build the binary and ensure it is named `whisper-cli`.
*   **Models:** Models (`.bin` files) go into a `Models` subfolder: `[Data Folder]/SpeechToText/Cpp/Models`.

### Purfview Faster-Whisper (GPU Speech-to-Text)
Used for GPU-accelerated AI-based speech recognition.

*   **Download:** [Purfview/whisper-standalone-win releases](https://github.com/Purfview/whisper-standalone-win/releases)
*   **Destination:** `[Data Folder]/SpeechToText/Purfview-Faster-Whisper-XXL`
*   **Files:** Download the Linux Archive, extract so `faster-whisper-xxl` binary is present.
*   **Models:** Place model directories (e.g., `faster-whisper-medium`) inside the `_models` folder.

### SE5 Speech-to-Text, OCR, and TTS Engines

The same data-folder layout is used on Linux. Prefer the in-app downloaders for Crisp ASR, Qwen3 ASR, Parakeet.cpp, PaddleOCR, Qwen3 TTS, and Kokoro TTS because the required files differ by build and model.

---

## macOS

### FFmpeg
Used for reading media info, extracting audio, and generating waveforms.

*   **Install:** Via Homebrew (e.g., `brew install ffmpeg`) or download static builds from [ffmpeg.org](https://ffmpeg.org/download.html).
*   **Files:** Subtitle Edit will look for `ffmpeg` in system paths (e.g., `/opt/homebrew/bin/ffmpeg`).
*   **Alternative:** Place the `ffmpeg` binary in `[Data Folder]/ffmpeg`.
*   **Custom Path:** You can specify a custom path in `Settings.json` (located in the Data Folder):
    ```json
    "FfmpegPath": "/path/to/your/ffmpeg"
    ```

### MPV Media Player (libmpv)
Used as a video player engine.

*   **Install:** Use Homebrew (e.g., `brew install mpv`).
*   **Files:** Subtitle Edit looks for `libmpv.dylib` or `libmpv.2.dylib` in standard library paths (`/opt/homebrew/lib`, `/usr/local/lib`, etc.).

### yt-dlp (Online Video Playback)
Used to enable mpv to stream online videos via **Video > Open from URL**.

*   **Download:** [yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)
*   **Destination:** `[Data Folder]` (The root data folder)
*   **File:** Download `yt-dlp_macos` and place it directly in the root of the Data Folder.
*   **Permissions:** The file must be executable. Run: `chmod +x yt-dlp_macos`

> **Tip:** Subtitle Edit can download yt-dlp automatically when you use **Video > Open from URL** for the first time.

### Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

*   **Install:** Use Homebrew (e.g., `brew install tesseract`).
*   **Files:** Subtitle Edit will detect the system installation. Ensure language data (`tessdata`) is also installed.

### Whisper CPP (Speech-to-Text)
Used for AI-based speech recognition.

*   **Download:** [ggerganov/whisper.cpp releases](https://github.com/ggerganov/whisper.cpp/releases) or build from source.
*   **Destination:** `[Data Folder]/SpeechToText/Cpp`
*   **Files:** Download or build the binary and ensure it is named `whisper-cli`.
*   **Models:** Models (`.bin` files) go into a `Models` subfolder: `[Data Folder]/SpeechToText/Cpp/Models`.

### SE5 Speech-to-Text, OCR, and TTS Engines

Some newer local engines are platform-specific or model-specific. Use the in-app downloaders where available, and check [Speech to Text](features/speech-to-text.md), [Text to Speech](features/text-to-speech.md), and [OCR](features/ocr.md) for current engine notes.
