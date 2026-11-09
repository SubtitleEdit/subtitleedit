# What's New in Subtitle Edit 5

Subtitle Edit 5 is a rewrite of Subtitle Edit on Avalonia, so it runs on Windows, Linux and macOS. The editing workflow from Subtitle Edit 4 is still there; this page lists only what Subtitle Edit 5 adds on top of it. Features carried over from Subtitle Edit 4 are not repeated here.

## Platform and Look

- **macOS** is a supported platform, next to Windows and Linux (with a Flatpak build).
- **Themes** - several light and dark themes and icon sets, chosen from preview cards, or follow the system theme (see [Settings → Appearance](settings.md#appearance)).
- **High-DPI aware** UI with separate **UI scale** and **Font scale** settings.
- **Search in Settings** finds any option, and Settings.json gets automatic backups.
- **In-app updates** - pick the stable or beta channel, and get a quiet notice at startup when a newer build is out (see [Settings → Updates](settings.md#updates)).
- On macOS: help topics in the **Help** menu search, macOS-standard default shortcuts, Apple Vision OCR and VideoToolbox encoders.

## Editing

- **Command palette** - **Edit → Command palette...** (Ctrl+Shift+K) finds any command by name and runs it (see [Command palette](command-palette.md)).
- **Custom shortcuts** built from steps: run commands, insert text, find and replace (see [Custom shortcuts](shortcuts.md#custom-shortcuts)).
- **AI assistant for the current line** - ask a local or online AI model about the selected line, or to fix errors, fit the reading speed or change the tone (see [AI assistant](ai-assistant.md)).
- **Assisted split** and **Assisted move** - ranked suggestions for where to split a line, or which words to move to the neighbor line, each with a preview (see [Assisted split](assisted-split.md)).
- **Error list** - **Tools → List errors...** collects everything the grid flags into one window and exports it (see [Error list](list-errors.md)).
- **Tools → Change formatting** adds or removes italic, bold, underline and more on the selected lines (see [Change formatting](change-formatting.md)).
- **Tools → Merge two subtitles** makes one bilingual subtitle, as SubRip or as ASSA with two styles (see [Merge two subtitles](merge-two-subtitles.md)).
- **Tools → Merge continuation lines** merges lines unless a line ends a sentence (see [Merge continuation lines](merge-continuation-lines.md)).
- **Tools → Remove/replace Unicode characters** - the Subtitle Edit 4 plugin, now built in (see [Remove/replace Unicode characters](remove-unicode-characters.md)).
- **Forced narrative lines** - a *Forced* column, *Toggle forced*, and **File → Export → Forced lines only...** to write only those lines.
- **Edit original** in place, with the original's unmatched lines shown as reference rows.

## Subtitle Grid and Text Box

- **Show formatting in the grid** - italic, bold and colors are drawn in the grid, or **Hide tags** shows tag-heavy ASSA lines as plain text (see [Formatting display](subtitle-grid.md#formatting-display)).
- **Tag coloring** in the text box - HTML and ASSA tags are colored as you type, with live spell check, IME and right-to-left text still working.
- **Live spell check in the grid** - misspelled words are underlined, with suggestions in the context menu (see [Spell Check](spell-check.md)).
- **Columns...** to reorder and hide grid columns, plus optional *Shot in/out* columns.
- **Statistics dashboard** with KPI tiles, checks and a CPS histogram (see [Statistics](statistics.md)).
- **Time code mode "Frame numbers"** shows absolute frame numbers (see [Settings → General](settings.md#general)).
- Drag and drop text between the text boxes, and **Search via** shortcuts that look up the selected text on the web.

## Main Window and Layouts

- **Layout 14** - an editor-style timeline with a video thumbnail row and subtitle rows (by layer, actor or style) above the waveform (see [Layouts](main-window.md#layouts)).
- **Layouts 12 and 13** put the text box below the video player.

## Video and Waveform

- **FFmpeg video player** - hardware decoding, exact frame stepping (also variable frame rate), and an audio track picker (see [Supported video players](video-player.md#supported-video-players)).
- **Show speech only** waveform (see [Speech-only waveform](audio-visualizer.md#speech-only-waveform)), customizable waveform toolbar, and importable waveform themes.
- **Guess start / Guess end** move a line's edge to where the speech starts or stops on the waveform.
- **Video → More** has new tools:
  - **Cut video**, with transitions and fade from/to black (see [Cut video](cut-video.md)).
  - **Re-encode video for better subtitling** (see [Re-encode video](re-encode-video.md)).
  - **Remux video** - video, several audio tracks and subtitles into one container (see [Remux video](remux-video.md)).
  - **Chapters** editor for Matroska, MP4 and OGM (see [Chapters](chapters.md)).
  - **Generate background music** with local ACE-Step, looped to the video's length (see [Background music](background-music.md)).
- **Burn-in** adds a logo/watermark, [3D video](burn-in.md#3d-video) (half side-by-side / top-bottom), burn-in of selected lines, and hardware encoders for the current OS (see [Effect and logo](burn-in.md#effect-and-logo)).
- **PGS position monitor** in the image subtitle editor shows where every subtitle sits on the video canvas.

## Sync and Timing

- **Tools → Improve time codes (forced alignment)** re-times a roughly synced subtitle with a speech aligner (see [Improve time codes](improve-time-codes.md)).
- **Beautify time codes** is a live tool with before/after waveforms and a reason per changed line, and is also a batch convert step (see [Beautify time codes](beautify-time-codes.md)).
- **Visual sync** shows the waveform and the subtitle on the video.
- **Point sync via other subtitle** loads subtitles from mkv/mp4/.sup and shows gap badges (see [Point sync via other subtitle](point-sync-via-other.md)).

## Speech to Text

- **Crisp ASR** - one engine with many backends (Parakeet, Canary, Qwen3, Voxtral, GigaAM, SenseVoice, ReazonSpeech and more), downloaded and managed by Subtitle Edit.
- **Qwen3 ASR**, **Google Cloud Speech-to-Text v2**, and an **OpenAI-compatible** engine (also vLLM).
- **WhisperX** as a standalone build - no Python install needed.
- **Detect speakers** (diarization), a choice of VAD, and **Isolate speech** before transcribing (see [Isolate speech](speech-to-text.md#isolate-speech-crisp-asr)).
- **Forced-aligner picker** for word-level time codes, and **auto-detect language**.
- A **quality report** after each run (too short, too long, overlapping, non-speech and repeated lines).

See [Speech to Text](speech-to-text.md).

## Text to Speech

- **New local engines** - Qwen3 TTS, Kokoro, OmniVoice, IndexTTS 2.5, Chatterbox, CosyVoice3, KugelAudio, Supertonic, Higgs Audio, Fish Audio S2 Pro, VibeVoice, VoxCPM2 and more, downloaded and managed by Subtitle Edit.
- **New online engines** - Edge-TTS, Mistral TTS and an OpenAI-compatible engine (see [Supported engines](text-to-speech.md#supported-engines)).
- **Voice cloning from the video** - find every speaker and clone them all, or clone the voice of each line.
- **Remove original speech** - keeps the music and effects under the new voices (see [Remove the original speech](text-to-speech.md#remove-the-original-speech-from-the-video)).
- **Play with speech** and **Speak from current line** read the subtitle aloud in sync with the video (see [Play with speech](text-to-speech.md#play-with-speech)).
- **Voice manager** to copy voices between engines and share voice packs (see [Voice manager](text-to-speech.md#voice-manager)).
- Speaker names from SDH text go to the actor field, sound and music lines can be left silent, and custom Piper voices can be imported.

See [Text to Speech](text-to-speech.md).

## Auto-translate

- **Server-managed llama.cpp** - Subtitle Edit downloads llama.cpp and the models and runs the server, so translation works offline with no setup.
- **CrispASR MADLAD**, a local translation engine with downloadable models.
- **OpenAI-compatible API** for any `chat/completions` endpoint, plus **LM Studio** model browsing and **API Route**.
- **Advanced engines** (llama.cpp, Ollama, LM Studio) translate in batches with surrounding context, a synopsis and a glossary (see [Advanced auto-translate](auto-translate-advanced.md)).
- **Translate in place**, retry of lines that come back untranslated, and lines re-broken for the target language.

See [Auto-translate](auto-translate.md).

## AI Review

- **Tools → AI review** - an AI proofreading pass for typos, spelling, grammar, punctuation and casing that does not rephrase.
- Runs locally by default (llama.cpp), or via Ollama or any OpenAI-compatible endpoint.
- Suggestions are listed before/after with a reason and a category; tick the ones to apply. Formatting tags and line breaks are kept.

See [AI Review](ai-review.md).

## OCR

- **New engines** - **CrispEmbed** (local GLM-OCR, Qwen3-VL, PP-OCRv6, DeepSeek-OCR-2 and more), **llama.cpp vision models** (including your own `.gguf`), **Apple Vision** on macOS and **Mistral OCR**.
- **Video OCR** - **Video → OCR burned-in subtitle** reads hardcoded subtitles from a video (see [Video OCR](video-ocr.md)).
- **nOCR** is more accurate (scale-aware matching, automatic spaces, a better Latin database) and glyphs can carry their own italic (see [nOCR](ocr.md#nocr-nikse-ocr)).
- **Auto-detected OCR language**, histogram-based color isolation for DVD subtitles, and **Find and Replace** in the OCR window (see [Find and Replace](ocr.md#find-and-replace)).

See [OCR](ocr.md).

## Subtitle Formats and Import

- **New formats** - EBU-TT-D, EBU-TT, IMSC Rosetta, YouTube timed text srv3 (`.ytt`), Manzanita DVB teletext, CANVASs SSTG1, Wistia json, DVD Junior SPC and Sonic DVD Producer.
- **New image exports** - IMSC 1.1 image profile and DVD sup (MuxMan/Scenarist).
- **New exports** - Final Cut Pro XML Captions and DaVinci Resolve marker EDL.
- **CEA-608/708 closed captions** from `.ts`, `.mkv`, `.mp4`, `.mxf`, `.vob`/`.mpg` and `.m2v`.
- Read subtitles from **fragmented MP4**, **ARIB STD-B24** captions, **SMPTE-TT bitmap** captions and **XSUB** in `.avi`.
- **Import CSV/XLSX/ODS** with custom column mapping (see [Import spreadsheets](import-csv-xlsx.md)), a **generic XML importer**, and Premiere Pro marker CSV.
- **EBU STL** and teletext preview positions and boxes as a decoder would, and **SCC** writes colors as mid-row codes.

## ASSA

- **ASSA draw** is built in, with a shape library, scale/rotate, backgrounds and SVG import (see [ASSA draw](assa-draw.md)).
- **Apply advanced effects** - typewriter, karaoke, neon, glitch, snow, lower third and many more, with a live preview (see [Advanced effects](assa-apply-advanced-effects.md)).
- **Font collector** - checks the script's fonts, copies them, or embeds them with unused glyphs trimmed (see [Attachments](assa-attachments.md)).
- **Hide layer** in the preview, and filter the grid by style, actor, layer or tag.

## Batch Convert

- **Translate into several languages** in one run, with the local translate engines.
- **Named presets** and filters by format, extension, error status and forced tracks.
- AI OCR engines (CrispEmbed, llama.cpp, Ollama, Paddle) and new steps such as *Change formatting*, *Beautify time codes* and *Snap time codes to frames* (see [Available functions](batch-convert.md#available-functions)).

## Command Line (seconv)

- `seconv` is part of the main repository and ships with every release.
- Colored progress and summary tables, and `--json` output for scripts and CI.
- `info` and `lint` subcommands, AI OCR and translate engines, custom output templates, and POSIX-style flags (the Subtitle Edit 4 flags still work).

See [Command Line (seconv)](../reference/command-line.md).

## Where to Look Next

- [Main Window](main-window.md) - the application layout.
- [Audio Visualizer / Waveform](audio-visualizer.md) - waveform, spectrogram and shot changes.
- [Third-Party Components](../third-party-components.md) - component setup and data folders.
