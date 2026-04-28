# What's New in Subtitle Edit 5

Subtitle Edit 5 is the Avalonia-based, cross-platform version of Subtitle Edit. It keeps the familiar subtitle editing workflow from the Windows Forms version, but many feature areas were rebuilt or expanded.

## Application Platform

- Cross-platform Avalonia UI for Windows, Linux, and macOS.
- Updated video playback integration with libmpv/VLC support.
- New Flatpak packaging work for Linux.
- More windows remember their size and position.
- More complete keyboard shortcut coverage, including single-key workflow settings.

## Video and Waveform

- Waveform toolbar visibility can be toggled from the Video menu.
- Waveform toolbar buttons can be customized, sorted, imported, and exported.
- Waveform themes can be imported and exported.
- Spectrogram display can be generated and combined with the waveform view.
- Shot change colors can be customized.
- Embedded subtitle tracks can be added, removed, previewed, and edited for Matroska/WebM videos.
- Video tools now include burn-in, transparent subtitle video, blank video generation, re-encode, cut, and embedded subtitle editing workflows.

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
- PaddleOCR, Ollama OCR, Mistral OCR, Google Lens, Google Vision, Azure Vision, Amazon Rekognition, and Llama.cpp OCR are available in the OCR workflow.
- The command-line converter `seconv` can run subtitle conversion and OCR without the GUI.

See [OCR](ocr.md), [Batch Convert](batch-convert.md), and [Command Line (seconv)](../reference/command-line.md).

## ASSA Tools

- More ASSA advanced effects are available.
- Custom override tags include history support.
- ASSA background boxes, resolution resampling, drawing, positioning, style editing, attachments, and image color picking are available as dedicated feature pages.

## Where to Look Next

- [Main Window](main-window.md) - updated application layout.
- [Audio Visualizer / Waveform](audio-visualizer.md) - waveform, spectrogram, and shot change tools.
- [Embedded Subtitles](embedded-subtitles.md) - SE5 Matroska/WebM embedded track editor.
- [Third-Party Components](../third-party-components.md) - component setup and data folder locations.
- [Command Line (seconv)](../reference/command-line.md) - headless conversion and OCR.
