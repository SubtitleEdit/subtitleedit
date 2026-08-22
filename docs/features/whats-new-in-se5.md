# What's New in Subtitle Edit 5

Subtitle Edit 5 is the Avalonia-based, cross-platform version of Subtitle Edit. It keeps the familiar subtitle editing workflow from the Windows Forms version, but many features were expanded.

## Application Platform

- Cross-platform Avalonia UI for Windows, Linux, and **macOS** — macOS is a new supported platform in SE 5.
- Cleaner, High-DPI-aware UI that scales correctly on modern displays.
- Follow system theme (light, dark, etc.) automatically, or pick a theme manually.
- New Flatpak packaging work for Linux.
- Many new settings, and the settings dialog has a built-in search to quickly find any option.
- Native pick-folder dialog wherever a folder is needed (was missing in the WinForms 4.x line).
- **Update check settings** — pick the stable or beta channel, and get a passive notification at startup when a newer build is out.
- **Proxy settings** — domain, system credentials and a bypass list, used by every download and online engine.
- **Type-to-search in every combo box**, a default save location setting, and the subtitle file name in dialog title bars.

## Editing and Grid

- **Show formatting in grid** — formatting tags (italic, bold, color, etc.) can now be rendered visually in the subtitle grid.
- **Tag coloring in the subtitle text box** — HTML and ASSA tags are syntax-highlighted while you type, using a native text box so centering, IME/CJK input, right-to-left text, live spell check (red underlines, suggestions, add to dictionary, ignore all) and the full context menu all keep working with color tags enabled.
- Edit controls Show/Hide/Duration are now optional and can be toggled on/off.
- Deleting many lines at once in the subtitle grid / list view is dramatically faster.
- New **Tools → Change formatting** dialog for adding or removing italic, bold, underline, and other formatting across selected lines.
- New **Tools → Merge two subtitles** tool that combines two subtitles (or the loaded subtitle's text + translation) into one bilingual subtitle. Output as SubRip (overlapping pairs stacked as line 1 / line 2) or ASSA with two configurable styles (font, color, outline, shadow, top/bottom alignment) and a live preview.
- **AI assistant for the current line** — ask a local AI model about the selected line or ask it for a change (fix errors, fit reading speed, more formal/casual, or a free-form request), from the text box context menu or the edit-box toolbar. Runs via llama.cpp, Ollama, or any OpenAI-compatible endpoint.
- **Edit original** — the original/reference column can be edited in place, and its non-matching lines are shown as reference rows in the grid.
- **Hide tags** — a third grid formatting mode that hides override tags so tag-heavy ASSA lines read as plain text; *show formatting* now shows the text of tag-heavy lines too.
- **Layout 10** puts the edit box under the waveform, like SE 4 and Aegisub.
- New **Tools → Remove/replace Unicode characters** (the SE 4 plugin, built in), **Sentence case** in the casing options, and the SE 4 **Minimum gap frame rate calculator**.
- **Multiple replace** grew up — import/export of selected rule categories, move rules up/down/to top/bottom, select all/none/invert in the preview, the whole rule shown while editing, remembered expanded categories, and a toolbar button.
- **Merge lines: keep end time** (allow overlap with the next subtitle) as an option, and *Show selected lines earlier/later* in the grid context menu.
- Auto-break settings under Settings → Tools, with a **do-not-break-after** list editor and a bottom-heavy percentage.
- Keyboard parity with SE 4: *Go to first/last line* with video sync, *Go to next empty line*, *toggle custom tags*, recalculate duration, Ctrl+Shift+Home/End selection, Shift+Backspace as forward delete, and *Copy/Paste (alternative)* text box shortcuts for clipboard managers.

## Spell Check

- **Live spell check in the subtitle grid** — misspelled words are underlined in the grid, and the grid context menu offers suggestions, *Add to user dictionary* and *Ignore all* (when a single line is selected).

See [Spell Check](spell-check.md) for details.

## Video

- New **Video → Re-encode** tool that re-encodes a video into a format better suited for subtitling work.
- New **Video → Cut video** tool for trimming video segments directly from Subtitle Edit.
- **Video → Burn-in with logo** — a logo/watermark image can now be included when burning subtitles into video.
- Many new speech-to-text engines (see [Speech to Text](#speech-to-text) below).
- New **Video → OCR burned-in subtitle** tool that extracts hardcoded subtitles from video via OCR (see [Video OCR](video-ocr.md)).
- Improved reading of subtitles embedded in MP4 files.
- **PGS position monitor** — the binary/image-based edit window has a BDSup2Sub-style position map for image-based subtitles (`.sup`/`.sub`/BDN): every subtitle's real rectangle on the video canvas, color coded by zone (active picture / letterbox bars), with configurable content aspect ratio, custom bar height, and title-safe margin.
- New **Video → More → Chapters** editor with Matroska, MP4 and OGM chapter formats (see [Chapters](chapters.md)).
- **Margin is part of the subtitle area** — the preview subtitle may use the letterbox bars, keeping a translation clear of burned-in forced narrative.
- **Burn-in** gains Apple VideoToolbox hardware encoders on macOS, `.webm`/`.ts` output, letter spacing, and lists only the encoders and containers the OS and codec actually support.
- **Matroska track chooser** shows an image-subtitle preview, the number of forced cues, and can export VobSub directly.
- *Go to sub position and pause* and *Go to video position…* shortcuts, and a toggle-subtitles shortcut for the video player.

## Sync

- **Visual Sync** now includes a waveform, making it easier to pick precise sync points.
- **Visual sync** and **Set sync point** show the subtitle on the video, and Visual sync finds or opens a video when only a subtitle is loaded.

## Waveform and Spectrogram

- Waveform toolbar buttons can be customized, sorted, imported, and exported.
- Waveform themes can be imported and exported.
- **Spectrogram style** can be changed at runtime — no re-generation needed.
- More customization options for the waveform and spectrogram, including colors, shot-change colors, and visual style.
- **Snap to shot changes** is consistent across every way of doing it — dragging an edge, dragging a whole subtitle, and the snap shortcuts all land the cue the *Beautify time codes* profile's in/out cues gap away from the cut. Dragging snaps when the cue *looks* close (a pixel distance, so it feels the same at any zoom), and the snap distances are now in Options → Settings → Waveform. **Shift** while dragging temporarily disables the snap.

## Beautify Time Codes

- New **Tools → Beautify time codes…** brings the SE 4 beautifier across, but as a live tool: two stacked waveform visualizers (original / beautified) show the result before you accept it, with prev/next navigation, frame and millisecond deltas, and a per-cue reason line (*snapped to shot change* · *min. gap enforced* · *min. duration enforced*, etc.).
- The full **profile editor** (zones, chaining, connected-subtitle handling, per-cue gap, presets for Netflix and SDI) is available from the tool window and from Options → Settings → Waveform. Profile edits persist into `Settings.json`.
- **Beautify time codes** is also a batch convert step, so a whole folder can be snapped to shot changes and frames in one run.

## OCR

Beyond the classic engines, image-to-text now includes AI-based local and online options:

- **CrispEmbed** — a new local ggml-based OCR engine (same family as CrispASR) with GLM-OCR, GOT-OCR2 and Qwen3-VL backends, downloaded and managed by Subtitle Edit.
- **llama.cpp vision models** — OCR through a server-managed llama.cpp instance, also available in Batch Convert and `seconv`.
- More engines overall: Tesseract, nOCR, Binary OCR, Google Lens, Google Vision, Ollama, Mistral OCR, and Paddle OCR (standalone or Python).
- **Show only forced subtitles** — a filter plus a *Forced* column, so only the forced lines are OCR'ed and returned (SE 4 parity).
- **Histogram-based color isolation** for VobSub/DVD subtitles, making it easier to separate text from borders and background.
- The spell-check dictionary is auto-selected from the OCR language, and the OCR fix replace lists are applied even when no Hunspell dictionary is installed.
- **Train nOCR** from SE 4 is back, and nOCR matching is faster thanks to a result cache.
- **HunyuanOCR 1.5** joins the llama.cpp OCR model list, with install-status dots for engines and models and an engine download/update button in the settings.
- **Save all images with HTML index** from the OCR window, and CrispEmbed's hardware build can be re-picked after the first install.

See [OCR](ocr.md) and [Video OCR](video-ocr.md) for details.

## Speech to Text

Speech recognition is no longer limited to classic Whisper workflows. Subtitle Edit 5 includes a broader set of local and downloadable engines:

- Purfview Faster-Whisper XXL, CTranslate2, Whisper.cpp (with cuBLAS and Vulkan backends on Windows), OpenAI Whisper, OpenAI-compatible STT, and Const-me's Whisper.
- Qwen3 ASR with multiple GGUF model sizes.
- Crisp ASR variants including GLM, Qwen3, Granite, Omni, Parakeet, Canary, Cohere, Fire Red, Mega, and Kyutai.
- Forced-aligner picker (built-in / Canary CTC / Qwen3 / 12 language-specific wav2vec2 aligners) for word-level timestamps.
- Per-engine advanced parameters and batch transcription improvements.
- Automatic language selection for several newer engines.
- **Voxtral** backend for Crisp ASR, a way to switch VAD off, and an automatic retry without VAD when a clip comes back empty.
- Batch speech to text can include the language code in the output file names.

See [Speech to Text](speech-to-text.md) for the current engine list and workflow.

## Text to Speech

Text to speech now includes more local and cloud engines:

- Edge-TTS.
- Mistral TTS.
- Qwen3 TTS with downloadable local server builds and models.
- Kokoro TTS with downloadable local server builds and models.
- OmniVoice TTS - a local CPU engine (many languages, voice cloning) with downloadable models.
- Review audio clips, regenerate individual lines, keep regeneration history, and export generated clips with metadata.
- **IndexTTS 2.5** — a local engine with emotion and speaking-rate control.
- **Find voices in video and clone them all** — auto-cast every speaker from the video, or clone the voice of a single line from the video.
- Chatterbox in 23 languages (plus F16 and Q4_K model variants), and the CosyVoice3 RL talker models.

See [Text to Speech](text-to-speech.md) for details.

## Auto-translate

Subtitle Edit 5 adds local, downloadable auto-translate engines that run entirely on your own machine:

- **Server-managed llama.cpp** — Subtitle Edit downloads llama.cpp, manages a local `llama-server` process, and offers a curated TranslateGemma model picker, so no manual server setup is required. CPU, Vulkan, and CUDA builds are available, and the server can be started and stopped from the Auto-translate window.
- **CrispASR MADLAD** — a local MADLAD-based translation engine with downloadable models (shown with size and install status), available in both the Auto-translate window and Batch Convert.
- **OpenAI Compatible API** — a generic engine for any service exposing an OpenAI-compatible `chat/completions` endpoint (vLLM, KoboldCpp, a llama.cpp server on another machine, cloud providers, ...).
- **MiLMMT-46** translation models in the llama.cpp engine, and a **llama.cpp advanced** engine with custom prompt, server parameters, a stall watchdog and a token cap.
- Completion-format prompts for LM Studio, KoboldCpp, Ollama and the other local engines, a reset-to-default button for the prompt, and the chosen languages are kept when the engine changes.

See [Auto-translate](auto-translate.md) for the full engine list and workflow.

## AI Review

- New **Tools → AI review** — an AI proofreading pass that catches typos, spelling, grammar, punctuation, and casing errors without rephrasing or changing meaning, tone, or style.
- **Runs locally by default** — uses a server-managed llama.cpp engine with a downloadable model picker, so no cloud service or API key is required. Ollama and any OpenAI-compatible endpoint are also supported.
- **Review before you apply** — suggestions are listed as before/after pairs with a per-line reason, grouped by category (Spelling, Grammar, Punctuation, Casing, Other) with filter chips. Tick the ones you want and apply only those.
- **Safe by design** — formatting tags (`<i>`, `{\an8}`, etc.) and line breaks are preserved; suggestions that touch tags are dropped, and large rewrites are flagged for a closer look and left unselected.
- **Editable prompt** — the instructions sent to the model can be customized (with the subtitle language auto-detected and substituted in).
- **Play current**, **Select none**, and a **Start/Stop server** button so the local model's VRAM can be released without leaving the window; the server is also stopped when a review is cancelled. AI review is in the grid's *Selected lines* context menu too.

See [AI Review](ai-review.md) for details.

## Batch conversion

- **OCR while converting** — Batch Convert can turn image-based subtitles into text-based formats in bulk, using nOCR, Binary OCR, Tesseract, Ollama, llama.cpp vision models, or PaddleOCR. Language and pixels-are-space settings can be auto-detected for nOCR/Binary OCR, so converting many files with similar fonts needs far less manual setup.
- **Local auto-translate in the queue** — the new local engines (server-managed llama.cpp / TranslateGemma and CrispASR MADLAD) can be applied directly as a batch conversion step, fully offline.
- **More chainable functions** — including the new *Change formatting* (add/remove italic, bold, underline, etc.) alongside the existing fixes, replacements, casing, time-code, gap, merge, and split operations.
- **Speech-to-text batch mode** — transcribe many media files at once and save the results next to the source files.
- **Optimized MKV parsing** — reading subtitle tracks from Matroska (`.mkv`) files is significantly faster, speeding up batch jobs that extract subtitles from many video containers.
- **Beautify time codes**, **Convert colors to dialog**, **Snap time codes to frames**, and the **CrispEmbed** OCR and **llama.cpp advanced** translate engines as batch steps; the llama.cpp launch flags are yours to set.
- **Add folder…**, a filtered file count, translation progress, and an option to **keep the source file's date/time** on converted files (also in `seconv`).

See [Batch Convert](batch-convert.md), [OCR](ocr.md), and [Command Line (seconv)](../reference/command-line.md).

## ASSA Tools

- New **Apply advanced effects** tool that generates cinematic and creative ASSA override-tag animations (typewriter, karaoke, bounce-in, neon, glitch, rainbow, starfield, rain, snow, fireflies, and more) with real-time video preview.
- **Hide layer** — individual ASSA layers can now be hidden in the preview to focus on the lines you are working on.
- **ASSA filtering** — filter and search lines in the ASSA grid by style, actor, layer, or tag content.
- **Font collector** — shows the fonts a script uses and whether they are available, copies them to a folder, and can embed them in the subtitle with unused glyphs trimmed (see [Styles](assa-styles.md) and [Attachments](assa-attachments.md)).
- More advanced effects: **Word flip 3D**, **Lower third** and **Cinematic title**.

## Subtitle Formats

- Added **EBU-TT-D** (read and write) — the TTML distribution profile used by European broadcasters (BBC iPlayer, ARD/ZDF, NPO) and HbbTV.
- Added **IMSC-Rosetta Timed Text** subtitle format support.
- New **IMSC 1.1 image profile** export — a single self-contained TTML file with base64 PNG subtitles (`smpte:image`) for streaming and broadcast image-subtitle delivery, from File → Export and from the image/binary subtitle editor.
- New **DVD sup (MuxMan/Scenarist)** image-based export — the classic DVD-Video subpicture `.sup` that DVD authoring tools import.
- New **Import CSV/XLSX with custom columns** window for spreadsheets that don't fit the standard layout — pick which columns map to start, end, text, etc.
- **Teletext / EBU STL** — an alignment dialog, a *TT* grid column, a teletext color picker with a *No color* option, and the position the file carries is used when previewing on the video (also for TTML and PAC).
- New exports: **Final Cut Pro XML Captions**, **DaVinci Resolve marker EDL** (import too), **Audacity/Tenacity labels**, **BDN/xml 8-bit** with palette-indexed PNGs, and the **Full frame image** option is back.
- New imports: Adobe Premiere Pro *Markers* panel CSV, and **XSUB** subtitles inside `.avi` files.
- **D-Cinema interop** properties dialog, and the option to remove blank lines when opening a subtitle is back from SE 4.
- Abbreviation lists for 30 more languages.

## Command Line (seconv)

The `seconv` headless converter now lives in the main Subtitle Edit repository — it builds, ships, and updates in lockstep with the desktop app. 

- **Polished terminal UI** — colored output with progress per file, summary tables, and a `--json` mode for CI pipelines and scripting.
- **Cross-platform** — runs on Windows, Linux, and macOS with only the .NET runtime; no display or GUI required, suitable for servers and Docker.
- **Broader feature set** — additional time and cleanup operations, OCR engine selection (Tesseract / nOCR / Binary OCR / Ollama / PaddleOCR), container input from `.mkv` / `.mp4` / `.mcc`, `info` and `lint` subcommands for inspection, custom output templates, and POSIX-style flag names (legacy SE 4.x flags still work).
- `.avi`/XSUB input, rule selection for `--remove-formatting`, and keeping the source file's date/time on output.

See [Command Line (seconv)](../reference/command-line.md) for usage and examples.

## Where to Look Next

- [Main Window](main-window.md) - updated application layout.
- [Audio Visualizer / Waveform](audio-visualizer.md) - waveform, spectrogram, and shot change tools.
- [Third-Party Components](../third-party-components.md) - component setup and data folder locations.
- [Command Line (seconv)](../reference/command-line.md) - headless conversion and OCR.
