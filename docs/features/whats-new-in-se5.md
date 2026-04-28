# What's New in Subtitle Edit 5

Subtitle Edit 5 is the Avalonia-based, cross-platform version of Subtitle Edit. It keeps the familiar subtitle editing workflow from the Windows Forms version, but many feature areas were rebuilt or expanded.

## Application Platform

- Cross-platform Avalonia UI for Windows, Linux, and **macOS** — macOS is a new supported platform in SE 5.
- Cleaner, High-DPI-aware UI that scales correctly on modern displays.
- Follow system theme (light, dark, etc.) automatically, or pick a theme manually.
- New Flatpak packaging work for Linux.
- More windows remember their size and position.
- Settings dialog has a built-in search to quickly find any option.
- Native pick-folder dialog wherever a folder is needed (was missing in the WinForms 4.x line).

## Editing and Grid

- Deleting many lines at once in the subtitle grid / list view is dramatically faster.
- New **Tools → Change formatting** dialog for adding or removing italic, bold, underline, and other formatting across selected lines.

## Video

- New **Video → Re-encode** tool that re-encodes a video into a format better suited for subtitling work.
- New **Video → Cut video** tool for trimming video segments directly from Subtitle Edit.

## Sync

- **Visual Sync** now includes a waveform, making it easier to pick precise sync points.

## Waveform and Spectrogram

- Waveform toolbar visibility can be toggled from the Video menu.
- Waveform toolbar buttons can be customized, sorted, imported, and exported.
- Waveform themes can be imported and exported.
- Spectrogram display can be generated and combined with the waveform view.
- **Spectrogram style** can be changed at runtime — no re-generation needed.
- More and fancier waveform styles to choose from.
- More customization options for the waveform and spectrogram, including colors, shot-change colors, and visual style.

## Speech to Text

Speech recognition is no longer limited to classic Whisper workflows. Subtitle Edit 5 includes a broader set of local and downloadable engines:

- Whisper.cpp, cuBLAS, Vulkan, CTranslate2, Const-me's Whisper, OpenAI Whisper, and Purfview Faster-Whisper XXL.
- Qwen3 ASR with multiple GGUF model sizes.
- Parakeet.cpp models.
- Crisp ASR variants including GLM, Qwen3, Granite, Omni, Parakeet, Canary, Cohere, and Fire Red.
- Per-engine advanced parameters and batch transcription improvements.
- Automatic language selection for several newer engines.

See [Speech to Text](speech-to-text.md) for the current engine list and workflow.

## Text to Speech

Text to speech now includes more local and cloud engines:

- Edge-TTS.
- Mistral TTS.
- Qwen3 TTS with downloadable local server builds and models.
- Kokoro TTS with downloadable local server builds and models.
- Review audio clips, regenerate individual lines, keep regeneration history, and export generated clips with metadata.

See [Text to Speech](text-to-speech.md) for details.

## OCR and Batch Conversion

- nOCR and Binary OCR have improved matching and database workflows.
- Batch Convert can use Binary OCR and can auto-detect several nOCR/Binary OCR settings.
- PaddleOCR, Ollama OCR, Mistral OCR, Google Lens, Google Vision, and Llama.cpp OCR are available in the OCR workflow.
- The command-line converter `seconv` can run subtitle conversion and OCR without the GUI.

See [OCR](ocr.md), [Batch Convert](batch-convert.md), and [Command Line (seconv)](../reference/command-line.md).

## ASSA Tools

- New **Apply advanced effects** tool that generates cinematic and creative ASSA override-tag animations (typewriter, karaoke, bounce-in, neon, glitch, rainbow, starfield, rain, snow, fireflies, and more) with real-time video preview.
- **Hide layer** — individual ASSA layers can now be hidden in the preview to focus on the lines you are working on.

## Where to Look Next

- [Main Window](main-window.md) - updated application layout.
- [Audio Visualizer / Waveform](audio-visualizer.md) - waveform, spectrogram, and shot change tools.
- [Third-Party Components](../third-party-components.md) - component setup and data folder locations.
- [Command Line (seconv)](../reference/command-line.md) - headless conversion and OCR.
