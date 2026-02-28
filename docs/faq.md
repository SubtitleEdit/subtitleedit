# Frequently Asked Questions (FAQ)

## General

### What is Subtitle Edit?
Subtitle Edit is a free, open-source subtitle editor. It supports 300+ subtitle formats and provides tools for creating, editing, synchronizing, translating, and converting subtitles.

### Is Subtitle Edit free?
Yes. Subtitle Edit is released under the MIT license. It is completely free with no ads or limitations.

### What platforms does Subtitle Edit 5 run on?
Subtitle Edit 5 is built with Avalonia UI and runs on:
- Windows 10 and later
- Linux
- macOS (Apple Silicon / arm64)

### Where is my data stored?
Subtitle Edit stores settings and data in a platform-specific data folder. You can open it via **File → Open data folder** or the keyboard shortcut.

### How do I report a bug or request a feature?
Please open an issue on the [GitHub repository](https://github.com/SubtitleEdit/subtitleedit/issues).

---

## File Operations

### What subtitle formats are supported?
Subtitle Edit supports over 300 subtitle formats including SubRip (.srt), WebVTT (.vtt), Advanced SubStation Alpha (.ass), SubStation Alpha (.ssa), MicroDVD, SAMI, and many more. See [Supported Formats](reference/supported-formats.md) for the full list.

### How do I convert between subtitle formats?
1. Open the subtitle file via **File → Open**
2. Choose a new format from the format dropdown
3. Save via **File → Save as...**

For batch conversion, use **Tools → Batch convert...**.

### How do I import image-based subtitles?
Use **File → Import image subtitle for OCR...** to import Blu-ray SUP, VobSub, or other image-based subtitles and convert them to text using OCR.

You can use **File → Import image subtitle for edit...** to work/export the images.


---

## Video

### What video players are supported?
Subtitle Edit supports:
- **libmpv** — High-quality, wide format support
- **VLC** - Popular player, limited support in SE

Configure the video player in **Options → Settings → Video Player**.

If you have problems with a video, try **Video → More → Re-encode video for better subtitling**.

### How do I generate a waveform?
Open a video file, and Subtitle Edit will generate a waveform automatically if FFmpeg is installed.

To generate enable spectrogram go to **Options → Settings**.


### Where do I get FFmpeg?
Some functions in Subtitle Edit will prompt for downloading FFmpeg.

You can also download FFmpeg in **Options → Settings**.

---

## Speech to Text

### What speech-to-text engines are supported?
Subtitle Edit supports several Whisper-based engines:
- **Whisper.cpp** — Cross-platform, CPU-based
- **Whisper.cpp (cuBLAS)** — GPU-accelerated (Windows, NVIDIA)
- **Whisper.cpp (Vulkan)** — GPU-accelerated via Vulkan (Windows)
- **Purfview's Faster Whisper XXL** — Fast GPU/CPU-based (Windows, Linux)
- **CTranslate2** — Fast CPU/GPU-based 
- **Const-me's Whisper** — DirectX-based (Windows)
- **OpenAI Whisper** — Original Python implementation
- **Chat LLM cpp** — LLM-based transcription (Windows, Linux)

### How do I use Speech to Text?
1. Open a video file
2. Go to **Video → Speech to text...**
3. Choose an engine and model (download if needed)
4. Select the language
5. Click **Transcribe**

See [Speech to Text](features/speech-to-text.md) for details.

---

## Synchronization

### How do I fix subtitle timing that is off by a constant amount?
Use **Sync → Adjust all times...** and enter the offset in milliseconds.

### How do I sync subtitles that drift over time?
Use **Sync → Visual sync...** or **Sync → Point sync...** to synchronize at multiple points.

### How do I convert subtitles between different frame rates?
Use **Sync → Change frame rate...** and select the source and target frame rates.

---

## OCR

### What OCR engines are available?
- **Tesseract** — Open-source OCR engine with language packs
- **nOCR** — Nikse's built-in OCR with trainable databases
- **Binary OCR** — For binary image comparison
- **Google Lens OCR** — Cloud-based
- **Google Vision OCR** — Cloud-based
- **Ollama OCR** — Local LLM-based
- **Mistral OCR** — Cloud-based
- **PaddleOCR** — Local OCR

### How do I OCR Blu-ray subtitles?
1. Open the `.sup` file via **File → Open**
2. The OCR window will open automatically
3. Choose an OCR engine (Tesseract or nOCR recommended)
4. Start the OCR process

---

## Spell Check

### How do I install spell check dictionaries?
Go to **Spell check → Get dictionaries...** and download the dictionary for your language. Dictionaries are based on Hunspell/OpenOffice format.

### How do I add words to the dictionary?
During spell check, click **Add to dictionary** to add a word. You can also manage custom words in **Options → Word lists**.

---

## ASSA / Styling

### What is ASSA?
ASSA (Advanced SubStation Alpha) is a subtitle format that supports rich styling including fonts, colors, positioning, animations, and more. It is widely used in anime fansubbing and professional workflows.

### How do I edit ASSA styles?
Go to **ASSA → Styles...** to create and edit subtitle styles with fonts, colors, borders, shadows, and alignment.

---

## Keyboard Shortcuts

### How do I customize keyboard shortcuts?
Go to **Options → Shortcuts...** to view and customize all keyboard shortcuts.

### What are the default shortcuts?
See the [Keyboard Shortcuts Reference](reference/keyboard-shortcuts.md) for the complete default shortcut list.

---

## Troubleshooting

### Subtitle Edit won't play video
- Ensure you have a video player installed (libmpv or VLC)
- Check that the video player path is correctly set in **Options → Settings → Video Player**
- Try to re-encode the video for better compatibility via **Video → More → Re-encode video for better subtitling**

### Waveform is not showing
- Ensure FFmpeg is installed and the path is set in settings

### Speech to Text fails
- Ensure the Whisper engine is downloaded (it will prompt you)
- Ensure the model is downloaded
- Check the console log in the Speech to Text window for error details
- Try a smaller model if you run out of GPU memory
