# What's New in Subtitle Edit 5

Subtitle Edit 5 is the Avalonia-based, cross-platform version of Subtitle Edit. It keeps the familiar subtitle editing workflow from the Windows Forms version, but many features were expanded.

## Application Platform

- Cross-platform Avalonia UI for Windows, Linux, and **macOS** — macOS is a new supported platform in SE 5.
- Cleaner, High-DPI-aware UI that scales correctly on modern displays.
- Follow system theme (light, dark, etc.) automatically, or pick a theme manually.
- New Flatpak packaging work for Linux.
- Many new settings, and the settings dialog has a built-in search to quickly find any option.
- Native pick-folder dialog wherever a folder is needed (was missing in the WinForms 4.x line).

## Editing and Grid

- **Show formatting in grid** — formatting tags (italic, bold, color, etc.) can now be rendered visually in the subtitle grid.
- Edit controls Show/Hide/Duration are now optional and can be toggled on/off.
- Deleting many lines at once in the subtitle grid / list view is dramatically faster.
- New **Tools → Change formatting** dialog for adding or removing italic, bold, underline, and other formatting across selected lines.
- New **Tools → Merge two subtitles** tool that combines two subtitles (or the loaded subtitle's text + translation) into one bilingual subtitle. Output as SubRip (overlapping pairs stacked as line 1 / line 2) or ASSA with two configurable styles (font, color, outline, shadow, top/bottom alignment) and a live preview.

## Video

- New **Video → Re-encode** tool that re-encodes a video into a format better suited for subtitling work.
- New **Video → Cut video** tool for trimming video segments directly from Subtitle Edit.
- **Video → Burn-in with logo** — a logo/watermark image can now be included when burning subtitles into video.
- Many new speech-to-text engines (see [Speech to Text](#speech-to-text) below).
- Improved reading of subtitles embedded in MP4 files.

## Sync

- **Visual Sync** now includes a waveform, making it easier to pick precise sync points.

## Waveform and Spectrogram

- Waveform toolbar buttons can be customized, sorted, imported, and exported.
- Waveform themes can be imported and exported.
- **Spectrogram style** can be changed at runtime — no re-generation needed.
- More customization options for the waveform and spectrogram, including colors, shot-change colors, and visual style.
- **Snap to shot changes** now reads its snap window from the *Beautify time codes* profile's red zones, and **Shift** while dragging temporarily disables the snap.

## Beautify Time Codes

- New **Tools → Beautify time codes…** brings the SE 4 beautifier across, but as a live tool: two stacked waveform visualizers (original / beautified) show the result before you accept it, with prev/next navigation, frame and millisecond deltas, and a per-cue reason line (*snapped to shot change* · *min. gap enforced* · *min. duration enforced*, etc.).
- The full **profile editor** (zones, chaining, connected-subtitle handling, per-cue gap, presets for Netflix and SDI) is available from the tool window and from Options → Settings → Waveform. Profile edits persist into `Settings.json`.

## Speech to Text

Speech recognition is no longer limited to classic Whisper workflows. Subtitle Edit 5 includes a broader set of local and downloadable engines:

- Purfview Faster-Whisper XXL, CTranslate2, Whisper.cpp (with cuBLAS and Vulkan backends on Windows), OpenAI Whisper, OpenAI-compatible STT, and Const-me's Whisper.
- Qwen3 ASR with multiple GGUF model sizes.
- Crisp ASR variants including GLM, Qwen3, Granite, Omni, Parakeet, Canary, Cohere, Fire Red, Mega, and Kyutai.
- Forced-aligner picker (built-in / Canary CTC / Qwen3 / 12 language-specific wav2vec2 aligners) for word-level timestamps.
- Per-engine advanced parameters and batch transcription improvements.
- Automatic language selection for several newer engines.

See [Speech to Text](speech-to-text.md) for the current engine list and workflow.

## Text to Speech

Text to speech now includes more local and cloud engines:

- Edge-TTS.
- Mistral TTS.
- Qwen3 TTS with downloadable local server builds and models.
- Kokoro TTS with downloadable local server builds and models.
- OmniVoice TTS - a local CPU engine (many languages, voice cloning) with downloadable models.
- Review audio clips, regenerate individual lines, keep regeneration history, and export generated clips with metadata.

See [Text to Speech](text-to-speech.md) for details.

## Auto-translate

Subtitle Edit 5 adds local, downloadable auto-translate engines that run entirely on your own machine:

- **Server-managed llama.cpp** — Subtitle Edit downloads llama.cpp, manages a local `llama-server` process, and offers a curated TranslateGemma model picker, so no manual server setup is required. CPU, Vulkan, and CUDA builds are available, and the server can be started and stopped from the Auto-translate window.
- **CrispASR MADLAD** — a local MADLAD-based translation engine with downloadable models (shown with size and install status), available in both the Auto-translate window and Batch Convert.

See [Auto-translate](auto-translate.md) for the full engine list and workflow.

## AI Review

- New **Tools → AI review** — an AI proofreading pass that catches typos, spelling, grammar, punctuation, and casing errors without rephrasing or changing meaning, tone, or style.
- **Runs locally by default** — uses a server-managed llama.cpp engine with a downloadable model picker, so no cloud service or API key is required. Ollama and any OpenAI-compatible endpoint are also supported.
- **Review before you apply** — suggestions are listed as before/after pairs with a per-line reason, grouped by category (Spelling, Grammar, Punctuation, Casing, Other) with filter chips. Tick the ones you want and apply only those.
- **Safe by design** — formatting tags (`<i>`, `{\an8}`, etc.) and line breaks are preserved; suggestions that touch tags are dropped, and large rewrites are flagged for a closer look and left unselected.
- **Editable prompt** — the instructions sent to the model can be customized (with the subtitle language auto-detected and substituted in).

See [AI Review](ai-review.md) for details.

## Batch conversion

- **OCR while converting** — Batch Convert can turn image-based subtitles into text-based formats in bulk, using nOCR, Binary OCR, Tesseract, Ollama, or PaddleOCR. Language and pixels-are-space settings can be auto-detected for nOCR/Binary OCR, so converting many files with similar fonts needs far less manual setup.
- **Local auto-translate in the queue** — the new local engines (server-managed llama.cpp / TranslateGemma and CrispASR MADLAD) can be applied directly as a batch conversion step, fully offline.
- **More chainable functions** — including the new *Change formatting* (add/remove italic, bold, underline, etc.) alongside the existing fixes, replacements, casing, time-code, gap, merge, and split operations.
- **Speech-to-text batch mode** — transcribe many media files at once and save the results next to the source files.
- **Optimized MKV parsing** — reading subtitle tracks from Matroska (`.mkv`) files is significantly faster, speeding up batch jobs that extract subtitles from many video containers.

See [Batch Convert](batch-convert.md), [OCR](ocr.md), and [Command Line (seconv)](../reference/command-line.md).

## ASSA Tools

- New **Apply advanced effects** tool that generates cinematic and creative ASSA override-tag animations (typewriter, karaoke, bounce-in, neon, glitch, rainbow, starfield, rain, snow, fireflies, and more) with real-time video preview.
- **Hide layer** — individual ASSA layers can now be hidden in the preview to focus on the lines you are working on.
- **ASSA filtering** — filter and search lines in the ASSA grid by style, actor, layer, or tag content.

## Subtitle Formats

- Added **IMSC-Rosetta Timed Text** subtitle format support.
- New **Import CSV/XLSX with custom columns** window for spreadsheets that don't fit the standard layout — pick which columns map to start, end, text, etc.

## Command Line (seconv)

The `seconv` headless converter now lives in the main Subtitle Edit repository — it builds, ships, and updates in lockstep with the desktop app. 

- **Polished terminal UI** — colored output with progress per file, summary tables, and a `--json` mode for CI pipelines and scripting.
- **Cross-platform** — runs on Windows, Linux, and macOS with only the .NET runtime; no display or GUI required, suitable for servers and Docker.
- **Broader feature set** — additional time and cleanup operations, OCR engine selection (Tesseract / nOCR / Binary OCR / Ollama / PaddleOCR), container input from `.mkv` / `.mp4` / `.mcc`, `info` and `lint` subcommands for inspection, custom output templates, and POSIX-style flag names (legacy SE 4.x flags still work).

See [Command Line (seconv)](../reference/command-line.md) for usage and examples.

## Where to Look Next

- [Main Window](main-window.md) - updated application layout.
- [Audio Visualizer / Waveform](audio-visualizer.md) - waveform, spectrogram, and shot change tools.
- [Third-Party Components](../third-party-components.md) - component setup and data folder locations.
- [Command Line (seconv)](../reference/command-line.md) - headless conversion and OCR.
