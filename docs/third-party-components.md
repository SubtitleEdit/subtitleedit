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
*   **macOS:** `~/.config/Subtitle Edit` (or `$XDG_CONFIG_HOME/Subtitle Edit`).

> **Tip:** You can open the Data Folder directly from Subtitle Edit by pressing `Ctrl+Alt+Shift+D` (Windows/Linux) or `Cmd+Alt+Shift+D` (macOS).

---

## Windows

### Quick Reference Table

| Component | File(s) | Destination Path |
|-----------|---------|------------------|
| **FFmpeg** | `ffmpeg.exe`, `ffprobe.exe` (optional) | `[Data Folder]/ffmpeg` |
| **MPV** | `libmpv-2.dll` (+ `vulkan-1.dll`) | `[Data Folder]` (root) |
| **yt-dlp** | `yt-dlp.exe` | `[Data Folder]` (root) |
| **Tesseract** | `tesseract.exe`, `tessdata/` folder | `[Data Folder]/Tesseract` |
| **Whisper CPP** | `whisper-cli.exe`, `Models/` folder | `[Data Folder]/SpeechToText/Cpp` |
| **Purfview Faster-Whisper XXL** | `faster-whisper-xxl.exe`, `_models/` folder | `[Data Folder]/SpeechToText/Purfview-Faster-Whisper-XXL` |
| **Crisp ASR** | `crispasr.exe`, `models/` folder | `[Data Folder]/CrispASR` |
| **Qwen3 ASR CPP** | `qwen3-asr-cli.exe`, `models/` folder | `[Data Folder]/Qwen3ASR` |
| **PaddleOCR** | `paddleocr.exe`, `models/` folder | `[Data Folder]/OCR/PaddleOCR3-7` |
| **Qwen3 TTS (CrispASR)** | shares `crispasr.exe` + `models/` from `[Data Folder]/CrispASR`; reference voices in `voices/` | `[Data Folder]/TextToSpeech/Qwen3TtsCrispAsr` (voices only) |
| **Chatterbox TTS (CrispASR)** | shares `crispasr.exe` + `models/` from `[Data Folder]/CrispASR`; reference voices in `voices/` | `[Data Folder]/TextToSpeech/Chatterbox` (voices only) |
| **OmniVoice TTS** | `omnivoice-tts.exe`, `omnivoice-codec.exe`, `models/`, `voices/` | `[Data Folder]/TextToSpeech/OmniVoice` |
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
*   **Also needed: `vulkan-1.dll`** — builds from August 2026 onwards link the Vulkan loader dynamically, so `libmpv-2.dll` will not load at all without it (the symptom is a black video with no sound, while the waveform still works). It is normally installed by your graphics driver, but drivers for GPUs older than Vulkan — and the generic Microsoft Basic Display driver — do not provide it.
    *   Subtitle Edit's own "Download libmpv" button already includes it, so this only applies when you install libmpv manually.
    *   To get it: download the **Vulkan Runtime** components zip from [vulkan.lunarg.com](https://vulkan.lunarg.com/sdk/home#windows) and place `vulkan-1.dll` (the `x64` one, or the ARM64 build on ARM devices) next to `libmpv-2.dll` in the root of the Data Folder.

### yt-dlp (Online Video Playback)
Used to enable mpv to stream online videos (e.g., YouTube, Vimeo, and [many other sites](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md)) via **Video > Open from URL**.

*   **Download:** [yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases)
*   **Destination:** `[Data Folder]` (The root data folder)
*   **File:** Download `yt-dlp.exe` and place it directly in the root of the Data Folder.

> **Tip:** Subtitle Edit can download yt-dlp automatically. When you use **Video > Open from URL** for the first time, you will be prompted to download it.

### Tesseract OCR
Used for converting image-based subtitles (Sup/VobSub) to text.

*   **Download:** [UB-Mannheim Tesseract](https://github.com/UB-Mannheim/tesseract/wiki)
*   **Destination:** `[Data Folder]/Tesseract`
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

*   **Crisp ASR:** Stored in `[Data Folder]/CrispASR`. Models go into its `models` folder. Crisp ASR backends include Parakeet, Canary, Cohere, Fire Red, Fun-ASR Nano, GigaAM, GLM, Granite, Qwen3, Mega, MOSS Diarize, Omni, Kyutai, SenseVoice, ARK, and Voxtral.
    *   The speech-to-text dialog also offers a **Forced aligner** combo for word-level timestamps. Built-in (where the backend supports it), Canary CTC, Qwen3, and 12 language-specific wav2vec2 aligners (the WhisperX aligner zoo): `en`, `de`, `fr`, `es`, `it`, `ja`, `zh`, `nl`, `pt`, `ar`, `uk`, `cs`. The default is the built-in aligner when the backend supports it, otherwise Qwen3 or Canary CTC depending on the backend; pick a wav2vec2 entry manually to use one of those.
*   **Qwen3 ASR CPP:** Stored in `[Data Folder]/Qwen3ASR`. Models go into `[Data Folder]/Qwen3ASR/models`. (Parakeet is no longer a standalone engine; it is a Crisp ASR backend.)

Use [Speech to Text](features/speech-to-text.md) for the current engine list and workflow.

### PaddleOCR
Used for OCR of image-based subtitles.

*   **Destination:** `[Data Folder]/OCR/PaddleOCR3-7`
*   **Models:** `[Data Folder]/OCR/PaddleOCR3-7/models`
*   **Builds:** Subtitle Edit can download CPU, CUDA 11.8, or CUDA 12.9 builds, on both Windows and Linux (Linux x64 only; the Linux builds need glibc 2.35 or newer).
*   **Recognition models:** PP-OCRv6 (PaddleOCR 3.7) recognizes Chinese, English, Japanese and the Latin languages with one unified model; Arabic, Cyrillic, E-Slavic, Devanagari, Korean, Greek, Tamil, Telugu, Thai and Pali still use their PP-OCRv5 models, and Georgian its PP-OCRv3 one. All of them ship in the same bundle.
*   **Version:** The folder name follows the PaddleOCR release the standalone engine is built from, so upgrading installs into a new folder instead of mixing files. The old `[Data Folder]/OCR/PaddleOCR3-1` and `[Data Folder]/OCR/PaddleOCR3-4` folders are deleted automatically after the new one is installed.
*   **Paddle OCR Python:** The pip-installed engine uses the same downloaded models, so it needs `paddleocr` 3.7 or newer - older versions do not know the PP-OCRv6 model names.

### Local Text-to-Speech Engines
Subtitle Edit 5 can download local TTS servers and models from the **Text to speech** window.

*   **Qwen3 TTS (CrispASR):** Reference voices are stored in `[Data Folder]/TextToSpeech/Qwen3TtsCrispAsr/voices`. The talker GGUFs (VoiceDesign 1.7B, CustomVoice 1.7B or Voice clone Base 1.7B) and the 12 Hz codec are downloaded into the shared `[Data Folder]/CrispASR/models` cache alongside the Crisp ASR speech-to-text models, not under `TextToSpeech/Qwen3TtsCrispAsr/models` — installing Crisp ASR first is therefore recommended. Older installs that still have model files under the legacy `TextToSpeech/Qwen3TtsCrispAsr/models` folder are migrated automatically the first time the engine is used.
*   **Chatterbox TTS (CrispASR):** Reference voices are stored in `[Data Folder]/TextToSpeech/Chatterbox/voices`. The Base / Turbo model GGUFs (T3 + S3Gen) are downloaded into the shared `[Data Folder]/CrispASR/models` cache alongside the Crisp ASR speech-to-text models, not under `TextToSpeech/Chatterbox/models` — installing Crisp ASR first is therefore recommended. Older installs that still have model files under the legacy `TextToSpeech/Chatterbox/models` folder are migrated automatically the first time the engine is used.
*   **OmniVoice TTS:** Stored in `[Data Folder]/TextToSpeech/OmniVoice`. Brings its own `omnivoice-tts` and `omnivoice-codec` binaries. Supports 646 languages and voice cloning on CPU. `models/` and `voices/` subfolders.
*   **Kokoro TTS:** Stored in `[Data Folder]/TextToSpeech/KokoroTtsCpp`. Models go into the `models` folder.

These are examples, not the full set — many more local engines are downloadable from the Text to speech window (IndexTTS, CosyVoice3, dots.tts, VoxCPM2, MOSS-TTS, Zonos, VibeVoice, Confucius4-TTS, Pocket TTS, Higgs Audio, Fish Audio, and more), following the same layout: CrispASR-based engines share the `[Data Folder]/CrispASR` cache, and the rest live under `[Data Folder]/TextToSpeech/<engine>`.

Use [Text to Speech](features/text-to-speech.md) for the full engine list and engine-specific options.

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

The same data-folder layout is used on Linux. Prefer the in-app downloaders for Crisp ASR, Qwen3 ASR, PaddleOCR, and the local TTS engines because the required files differ by build and model.

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
