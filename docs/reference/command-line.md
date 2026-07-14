# Command Line (seconv)

`seconv` is Subtitle Edit's headless command-line converter. It reuses the same core libraries as the desktop app (`libse`, `libuilogic`), so it supports the same formats, operations, and OCR engines — without any GUI dependency. Useful for scripts, CI pipelines, server-side workflows, and bulk conversion.

`seconv` lives in the main Subtitle Edit repository and ships in lockstep with the desktop app — no separately maintained fork, no version drift.

## Highlights

- **380+ subtitle formats** — text, binary, and image-based.
- **Container input** — Matroska (`.mkv` / `.mks`), MP4, MCC, transport stream teletext, Blu-Ray `.sup`.
- **OCR for image-based sources** via five engines (Tesseract subprocess, nOCR built-in, BinaryOCR built-in, Ollama HTTP, PaddleOCR subprocess).
- **Auto-translate** via local LLMs (llama.cpp with automatic server start, Ollama, LM Studio) or self-hosted services (LibreTranslate, NLLB).
- **Image-based output** — Blu-Ray sup, BDN-XML, DOST, FCP (Final Cut Pro + image), D-Cinema interop / SMPTE 2014, images-with-time-code.
- **Operations pipeline** — offset, fps change, change-speed, renumber, adjust-duration, fix-common-errors, merge/split, balance, redo casing, RTL fixes, multiple-replace, custom-text format, plain text.
- **Cross-platform** — Windows, Linux, macOS. Only requires the .NET runtime; no display or GUI needed.

## Installation

Pre-built binaries are distributed alongside Subtitle Edit. To build from source:

```bash
dotnet build src/seconv/SeConv.csproj -c Release
```

The executable is `seconv` (or `seconv.exe` on Windows).

## Usage

```bash
seconv <pattern> <format> [options]
seconv <pattern> --format <name> [options]   # alternative syntax
```

The format may be passed as the second positional argument or via `--format <name>`. Multiple input patterns can be passed as separate quoted arguments or comma-separated:

```bash
seconv "file1.srt" "file2.srt" subrip --overwrite
seconv "*.srt,*.ass" subrip --input-folder:./in
```

Options accept either `--option:value`, `--option=value`, or `--option value`. The colon form is shown throughout this page.

### Quick examples

```bash
seconv *.srt sami                                                  # SRT → SAMI
seconv movie.srt subrip --encoding:windows-1252                    # encoding override
seconv movie.srt subrip --encoding:source                          # keep input's encoding on output
seconv *.sub subrip --fps:25 --output-folder:./out                 # frame-based → time-based

seconv movie.mkv subrip --track-number:3                           # extract MKV text track #3
seconv movie.sup subrip --ocr-engine:tesseract --ocr-language:eng  # OCR a Blu-Ray .sup
seconv movie.sup subrip --ocr-engine:nocr --ocr-db:Latin.nocr      # OCR via nOCR
seconv movie.sup subrip --ocr-engine:binaryocr --ocr-db:Latin.db   # OCR via BinaryOCR
seconv movie.sup subrip --ocr-engine:ollama --ollama-model:llama3.2-vision

seconv subs.srt bluraysup --resolution:1920x1080                   # render text → Blu-Ray sup
seconv subs.srt bdnxml --resolution:1920x1080                      # render text → BDN-XML
seconv subs.srt bluraysup --background-color:"#B4000000"           # ... with a black background box

seconv subs.srt customtext --custom-format:my-template.xml         # custom template
seconv *.srt subrip --multiple-replace:rules.xml                   # search-and-replace pass

seconv subs.srt subrip --offset:-2000 --renumber:1 --overwrite     # offset 2s back, renumber from 1
```

## Subcommands

```bash
seconv formats              # list all supported formats
seconv list-encodings       # list text encodings
seconv list-pac-codepages   # list PAC code pages
seconv list-ocr-engines     # list OCR engines + installation status
seconv list-fce-rules       # list FixCommonErrors rule IDs
seconv info <file>          # print format/encoding/duration/language for a file
seconv lint <pattern>       # validate subtitle(s); exit 1 if issues found
seconv --help               # show help
seconv --version            # print version and exit
```

### Inspect & validate

```bash
seconv info movie.srt                # human-readable table
seconv info movie.srt --json         # machine-parseable

seconv lint *.srt                    # check overlaps, line lengths, tags, ...
seconv lint *.srt --json             # CI-friendly: exit 1 on any issue
```

## Options

### File / I/O

| Option | Description |
|---|---|
| `--input-folder:<path>` | Input folder; relative patterns resolve against it |
| `--output-folder:<path>` | Output folder (default: input file's directory) |
| `--output-filename:<name>` | Output file name (single input only) |
| `--overwrite` | Overwrite existing files (default: rotate to `name_2.ext`, `_3.ext`, ...) |
| `--encoding:<name>` | Encoding name or codepage. Special values: `utf-8`, `utf-8-no-bom` (also `utf-8-nobom`, `utf8-nobom`), a code page number, or `source` to keep the input file's detected encoding. Defaults: auto-detect on input, UTF-8 BOM on output |

### Time / frame

| Option | Description |
|---|---|
| `--offset:hh:mm:ss:ms` | Shift all timecodes (also accepts plain ms, signed: `-1500`, `+1500`) |
| `--fps:<rate>` | Source frame rate (used for frame-based input formats) |
| `--target-fps:<rate>` | Target frame rate (with `--fps`, calls `Subtitle.ChangeFrameRate`) |
| `--adjust-duration:<ms>` | Add/subtract milliseconds to each paragraph's duration |
| `--change-speed:<percent>` | Scale all times by 100/percent (e.g. `125` = 1.25× faster). Must be > 0 |
| `--renumber:<n>` | Renumber paragraphs starting at `n` |

### Format-specific

| Option | Applies to | Description |
|---|---|---|
| `--resolution:<WxH>` | ASSA, image-based | Sets `PlayResX`/`PlayResY` for ASSA; sets canvas for image outputs (default `1920x1080`) |
| `--assa-style-file:<file>` | ASSA | Apply `[V4+ Styles]` block from another ASSA file |
| `--pac-codepage:<page>` | PAC | Code page name (`Latin`, `Greek`, `Hebrew`, …) or numeric (0–12). See `seconv list-pac-codepages` |
| `--ebu-header-file:<file>` | EBU STL | Reuse the GSI header block from an existing `.stl` file |
| `--plaintext-merge` | Plain text (`txt`) | Merge all subtitles into one space-separated block (no blank lines). Takes precedence over the two options below |
| `--plaintext-unbreak` | Plain text (`txt`) | Unbreak each subtitle, joining its lines into one |
| `--plaintext-no-blank-line` | Plain text (`txt`) | Do not put a blank line between subtitles (default keeps it) |

### Image output styling

When rendering a text subtitle to an image-based target (Blu-Ray `sup`, VobSub, BDN-XML, DOST, FCP / Final Cut Pro + image, D-Cinema, images-with-time-code, WebVTT thumbnails), these options control how the text is drawn. `<i>`, `<b>`, and `<font color=...>` tags in the subtitle text are honored. The same values can be set in a `--settings` JSON file's `exportImages` section (see [Settings JSON](#settings-json)); CLI flags win over the JSON.

| Option | Description |
|---|---|
| `--font-name:<name>` | Font family (default: `Arial`) |
| `--font-size:<pt>` | Font size in points (default: `50`) |
| `--font-color:<color>` | Text colour (default: `white`) |
| `--font-bold` | Render text bold |
| `--outline-color:<color>` | Outline colour (default: `black`) |
| `--outline-width:<px>` | Outline width (default: `2.5`; `0` disables) |
| `--shadow-color:<color>` | Shadow colour (default: `black`) |
| `--shadow-width:<px>` | Shadow width (default: `0` = off) |
| `--background-color:<color>` | Background box colour, e.g. `black` or `#B4000000` (semi-transparent black). Implies `--box-type:one-box` unless `--box-type` is given |
| `--background-corner-radius:<px>` | Corner radius of the background box (default: `0`) |
| `--box-type:<type>` | `none` (default) \| `one-box` \| `box-per-line` |
| `--box-padding:<px>` | Box padding: one value for all sides, or `left,right,top,bottom` (default: `5,5,3,3`) |
| `--line-spacing:<percent>` | Extra gap between lines as percent of line height (default: `0`) |
| `--alignment:<pos>` | Screen position: `bottom-center` (default), `top-left`, `middle-right`, ... |
| `--content-alignment:<align>` | Multi-line text justification: `left` \| `center` (default) \| `right` |
| `--bottom-top-margin:<px>` | Vertical screen-edge margin (default: 5% of height) |
| `--left-right-margin:<px>` | Horizontal screen-edge margin (default: 5% of width) |

Colours accept hex (`#AARRGGBB`, `#RRGGBB`, with or without `#`) or a colour name (`white`, `black`, `yellow`, ...).

```bash
# SRT → UHD Blu-Ray sup with a semi-transparent black background box (SE4-style)
seconv movie.srt bluraysup --resolution:3840x2160 --background-color:"#B4000000"

# Custom font, bold, box per line
seconv movie.srt bluraysup --font-name:Verdana --font-size:60 --font-bold --box-type:box-per-line
```

### Containers / tracks

`seconv` automatically extracts subtitle tracks from container files:

| Extension | Sources |
|---|---|
| `.mkv`, `.mks` | Matroska text tracks (S_TEXT/UTF8, SSA, ASS, HDMV/TEXTST) and image tracks (S_HDMV/PGS via OCR) |
| `.mp4`, `.m4v`, `.m4s`, `.3gp` | MP4 text tracks and WebVTT VTTC |
| `.mcc` | MacCaption 1.0 |
| `.ts`, `.m2ts`, `.mts` | Transport stream — teletext (no OCR) and DVB-sub (via OCR) |
| `.sup` | Blu-Ray sup (via OCR) |

When a container has multiple usable tracks, one output file is written per track with the track's language code as a suffix:

```
movie.mkv → movie.eng.srt
            movie.deu.srt
            movie.fra.srt
```

If two tracks share a language, the track number is added: `movie.#3.eng.srt`.

| Option | Description |
|---|---|
| `--track-number:<list>` | Comma-separated track numbers to keep |
| `--forced-only` | MKV: keep only forced tracks |
| `--teletext-only` | TS: skip DVB-sub OCR (teletext only) |
| `--teletext-only-page:<n>` | TS: extract only this teletext page |

### OCR

| `--ocr-engine` | Type | Setup |
|---|---|---|
| `tesseract` *(default)* | Subprocess | Install Tesseract (`apt install tesseract-ocr`, `brew install tesseract`, or the Windows installer); ensure it is on `PATH`. Pass `--ocr-language` as ISO 639-2 (`eng`, `deu`, `spa`, …). |
| `nocr` | In-process | Built-in nOCR matcher. Required: `--ocr-db:<path-to-Latin.nocr>`. |
| `binaryocr` *(alias: `binary`)* | In-process | Built-in BinaryOCR matcher (different accuracy profile, similar speed). Required: `--ocr-db:<path-to-Latin.db>`. |
| `ollama` | HTTP | Local Ollama server with a vision-capable model (e.g. `llama3.2-vision`, `qwen2.5vl`). Configure via `--ollama-url` (default `http://localhost:11434/api/chat`) and `--ollama-model` (default `llama3.2-vision`). Pass `--ocr-language` as a human name like `English`. |
| `paddle` *(alias: `paddleocr`)* | Subprocess | Install via `pip install paddleocr`; ensure the `paddleocr` binary is on `PATH`. Pass `--ocr-language` as a short code (`en`, `de`, …). |

| Option | Description |
|---|---|
| `--ocr-engine:<engine>` | `tesseract` (default) \| `nocr` \| `binaryocr` \| `ollama` \| `paddle` |
| `--ocr-language:<lang>` | Tesseract: ISO 639-2 (`eng`, `deu`); Paddle: short (`en`); Ollama: human (`English`) |
| `--ocr-db:<path>` | OCR database file: `.nocr` for `nocr`, `.db` for `binaryocr` (required for both) |
| `--dictionary-folder:<path>` | Folder with Hunspell dictionaries + `*_OCRFixReplaceList.xml`; enables the "Fix common OCR errors" pass of `--fix-common-errors` (English is bundled, so this is only needed for other languages) |
| `--ollama-url:<url>` | Default `http://localhost:11434/api/chat` |
| `--ollama-model:<model>` | Default `llama3.2-vision` |
| `--time-codes-only` | Image sources (`.sup`, VobSub `.sub`/`.idx`, MKV PGS/VobSub, MP4 VobSub, TS DVB-sub) → text format with time codes only and empty text. **Skips OCR entirely** — no OCR engine required. Ignored for text inputs and image output targets. |
| `--no-vobsub-isolate-colors` | Disable VobSub OCR colour isolation, which is **on by default**. Isolation rebuilds each subpicture as a crisp black-on-white bitmap via histogram-based colour analysis — the most frequent opaque colour (the glyph fill) becomes black and the gray outline / anti-alias colours collapse into the white background, which helps on discs whose outlines otherwise melt adjacent characters together (`Yuri` → `Yurl`). Pass this flag to OCR the raw palette instead. Ignored for non-VobSub sources and with `--time-codes-only`. |

> **OCR database files are not bundled with `seconv`.** The `nocr` and `binaryocr` engines need a `.nocr` or `.db` file passed via `--ocr-db`. Sources:
>
> - If you have the desktop UI installed: `%AppData%\Subtitle Edit\OCR\` (Windows) or `~/.config/Subtitle Edit/OCR/` (Linux/macOS).
> - From the repo: [`Ocr/Latin.nocr`](https://github.com/SubtitleEdit/subtitleedit/raw/main/Ocr/Latin.nocr) and [`Ocr/Latin.db`](https://github.com/SubtitleEdit/subtitleedit/raw/main/Ocr/Latin.db).
> - Other languages: download from the SE UI (Tools → "OCR with nOCR" / BinaryOCR → download).

Run `seconv list-ocr-engines` for the per-engine installation-status table.

```bash
# Tesseract
seconv movie.sup subrip --ocr-engine:tesseract --ocr-language:eng

# nOCR (no external dependency)
seconv movie.sup subrip --ocr-engine:nocr --ocr-db:"C:\Users\me\AppData\Roaming\Subtitle Edit\Ocr\Latin.nocr"

# BinaryOCR
seconv movie.sup subrip --ocr-engine:binaryocr --ocr-db:"C:\Users\me\AppData\Roaming\Subtitle Edit\Ocr\Latin.db"

# MKV with image (PGS or VobSub) tracks — OCR runs automatically
seconv movie.mkv subrip --ocr-engine:tesseract --ocr-language:eng

# VobSub .sub + .idx pair — the .idx companion is auto-detected
seconv movie.sub subrip --ocr-engine:tesseract --ocr-language:eng

# Transport-stream teletext (no OCR needed)
seconv broadcast.ts subrip

# Time codes only — extract timing with no OCR (empty text); works for any image source
seconv movie.sup subrip --time-codes-only
seconv movie.sub subrip --time-codes-only
```

### Auto-translate

`--translate-to:<language>` machine-translates each file as part of the conversion (after OCR for image sources, before the cleanup operations). Languages are given as a code or English name (`de`, `German`, `da`, `Danish`, …); the source language is auto-detected per file unless `--translate-from` is set.

| Option | Description |
|---|---|
| `--translate-to:<lang>` | Target language — enables translation |
| `--translate-from:<lang>` | Source language (default: auto-detect per file) |
| `--translate-engine:<engine>` | `llamacpp` (default) \| `ollama` \| `lmstudio` \| `libretranslate` \| `nllb-serve` \| `nllb-api` |
| `--translate-url:<url>` | Endpoint of an already-running translate server. For `llamacpp` this skips the local server auto-start; a bare `host:port` is completed to `/v1/chat/completions`. |
| `--translate-model:<model>` | `ollama`/`lmstudio`: model name. `llamacpp`: a `.gguf` file name from the models folder or a full path (default: the first installed translate model). |

**llama.cpp (default engine).** With no `--translate-url`, seconv runs a local `llama-server` for you: it looks for the binary in Subtitle Edit's data folder (`llama.cpp` next to `seconv`, then `%AppData%\Subtitle Edit\llama.cpp` / `~/Library/Application Support/Subtitle Edit/llama.cpp` / `~/.config/Subtitle Edit/llama.cpp`) and falls back to `llama-server` on `PATH`. The server is started on a free localhost port with the model's correct chat-template flags and stopped again when seconv exits. Models resolve against the data folder's `models` subfolder.

> **seconv never downloads engines or models** (same policy as Tesseract/PaddleOCR). Get llama.cpp + a translation model in one of these ways:
>
> - Run auto-translate with llama.cpp once in the Subtitle Edit UI (Tools → Auto-translate → llama.cpp) — it downloads both into the data folder that seconv probes.
> - Install llama.cpp yourself (`brew install llama.cpp`, `winget install ggml.llamacpp`, or a [GitHub release](https://github.com/ggml-org/llama.cpp/releases)) and drop a `.gguf` translation model (e.g. [TranslateGemma](https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF)) into the models folder, or pass it via `--translate-model:<path.gguf>`.
> - Point `--translate-url` at any llama-server you already have running.

```bash
# Translate to German via a local llama.cpp server (started and stopped automatically)
seconv movie.srt subrip --translate-to:de

# Specific source language and model
seconv movie.srt subrip --translate-from:en --translate-to:da --translate-model:translategemma-4b_Q4_K_M.gguf

# Already-running llama-server (local or remote)
seconv movie.srt subrip --translate-to:de --translate-url:http://192.168.1.10:8080

# Ollama
seconv movie.srt subrip --translate-to:da --translate-engine:ollama --translate-model:gemma2

# OCR a Blu-Ray sup and translate the result in one pass
seconv movie.sup subrip --ocr-engine:tesseract --ocr-language:eng --translate-to:de
```

### Templates / replacements

| Option | Description |
|---|---|
| `--multiple-replace:<path.xml>` | SE *MultipleSearchAndReplaceGroups* XML, applied per paragraph after operations. Supports `Normal` (case-insensitive), `CaseSensitive`, and `RegularExpression` rules |
| `--custom-format:<path.xml>` | SE *CustomFormatItem* XML (use with `--format customtext`) |
| `--settings:<path.json>` | JSON file overlaying `Configuration.Settings` (general / tools / removeTextForHearingImpaired) plus image-output styling (exportImages). Optional `profiles` map for named overlays |
| `--profile:<name>` | Selects a named overlay from the settings file's `profiles` map. Requires `--settings` |

#### Multiple-replace XML

```xml
<?xml version="1.0" encoding="utf-8"?>
<MultipleSearchAndReplaceGroups>
  <Group>
    <Name>Demo</Name>
    <IsActive>true</IsActive>
    <Rules>
      <Rule>
        <Active>true</Active>
        <FindWhat>colour</FindWhat>
        <ReplaceWith>color</ReplaceWith>
        <SearchType>Normal</SearchType>
      </Rule>
      <Rule>
        <Active>true</Active>
        <FindWhat>\bteh\b</FindWhat>
        <ReplaceWith>the</ReplaceWith>
        <SearchType>RegularExpression</SearchType>
      </Rule>
    </Rules>
  </Group>
</MultipleSearchAndReplaceGroups>
```

```bash
seconv *.srt subrip --multiple-replace:fixes.xml
```

#### Custom text format XML

```xml
<?xml version="1.0" encoding="utf-8"?>
<CustomFormatItem>
  <Name>JSON-ish</Name>
  <Extension>.json</Extension>
  <FormatHeader>{
  "lines": [</FormatHeader>
  <FormatParagraph>    {"start": "{start}", "end": "{end}", "text": "{text}"},</FormatParagraph>
  <FormatFooter>  ]
}</FormatFooter>
  <FormatTimeCode>hh:mm:ss.zzz</FormatTimeCode>
</CustomFormatItem>
```

```bash
seconv subs.srt customtext --custom-format:lines.xml
```

Available template tokens: `{title}`, `{number}`, `{start}`, `{end}`, `{duration}`, `{gap}`, `{text}`, `{text-line-1}`, `{text-line-2}`, `{actor}`, `{cps-comma}`, `{cps-period}`, `{text-length}`, `{bookmark}`, `{media-file-name}`, `{media-file-name-full}`, `{#lines}`, `{#total-words}`, `{#total-characters}`, `{tab}`. Time-code tokens accept `hh`/`h`, `mm`/`m`, `ss`/`s`, and `zzz`/`zz`/`z` for milliseconds (or `zzzzzzz...` for total milliseconds without breakdown).

#### Settings JSON

```json
{
  "general": {
    "subtitleLineMaximumLength": 43,
    "subtitleMinimumDisplayMilliseconds": 1000,
    "subtitleMaximumDisplayMilliseconds": 8000,
    "currentFrameRate": 23.976,
    "defaultFrameRate": 23.976,
    "minimumMillisecondsBetweenLines": 24,
    "maxNumberOfLines": 2,
    "mergeLinesShorterThan": 33,
    "subtitleMaximumCharactersPerSeconds": 25.0,
    "subtitleOptimalCharactersPerSeconds": 15.0,
    "subtitleMaximumWordsPerMinute": 400.0,
    "dialogStyle": "DashBothLinesWithSpace",
    "continuationStyle": "None"
  },
  "removeTextForHearingImpaired": {
    "removeTextBeforeColon": true,
    "removeInterjections": false
  },
  "exportImages": {
    "fontName": "Arial",
    "fontSize": 50,
    "fontColor": "#FFFFFF",
    "isBold": false,
    "outlineColor": "black",
    "outlineWidth": 2.5,
    "shadowColor": "black",
    "shadowWidth": 0,
    "backgroundColor": "#B4000000",
    "backgroundCornerRadius": 0,
    "boxType": "one-box",
    "boxPaddingLeft": 5,
    "boxPaddingRight": 5,
    "boxPaddingTop": 3,
    "boxPaddingBottom": 3,
    "lineSpacingPercent": 0,
    "alignment": "bottom-center",
    "contentAlignment": "center",
    "bottomTopMargin": 54,
    "leftRightMargin": 96
  },
  "profiles": {
    "broadcast": {
      "general": { "subtitleMaximumDisplayMilliseconds": 6000 },
      "removeTextForHearingImpaired": { "removeInterjections": true }
    },
    "uhd": {
      "exportImages": { "fontSize": 100 }
    }
  }
}
```

The `exportImages` section styles text → image rendering (see [Image output styling](#image-output-styling) for the semantics); the equivalent CLI flags override it. The `general` section mirrors `Configuration.Settings.General`; any key left out keeps the libse default. The profile-shaping values (`minimumMillisecondsBetweenLines`, `maxNumberOfLines`, `mergeLinesShorterThan`, `subtitleMaximumCharactersPerSeconds`, `subtitleOptimalCharactersPerSeconds`, `subtitleMaximumWordsPerMinute`, `dialogStyle`, `continuationStyle`) feed Fix common errors and the split/merge operations, so set them to reproduce an SE4 profile. `dialogStyle` and `continuationStyle` take the enum names (case-insensitive): `dialogStyle` ∈ `DashBothLinesWithSpace`, `DashBothLinesWithoutSpace`, `DashSecondLineWithSpace`, `DashSecondLineWithoutSpace`; `continuationStyle` ∈ `None`, `NoneTrailingDots`, `NoneTrailingEllipsis`, `OnlyTrailingDots`, `LeadingTrailingDots`, `LeadingTrailingEllipsis`, `LeadingTrailingDash`, … (see the Fix common errors continuation styles).

Keys that seconv does not recognize are ignored, so a settings file written for a newer version still applies everything this one understands — but they are listed in a warning, so a typo (or a key your seconv is too old to know) does not silently give you default output.

```bash
seconv *.srt subrip --settings:my.json --profile:broadcast --remove-text-for-hi
```

### Verbosity

| Option | Description |
|---|---|
| `--quiet` / `-q` | Suppress per-file progress and the parameters table; only print the final summary |
| `--verbose` / `-v` | Print extra diagnostic information, including full exception details (stack traces) on errors |
| `--json` | Emit per-file results as JSON to stdout (suppresses Spectre output) |

## Operations

Operations run after the structural transforms (offset, fps, renumber, adjust-duration, change-speed) in a fixed, sensible order regardless of where they appear on the command line:

| Option | Description |
|---|---|
| `--apply-duration-limits` | Apply duration limits |
| `--apply-min-gap[:<ms>]` | Enforce a minimum gap between paragraphs. Without a value, uses `minimumMillisecondsBetweenLines` from `--settings` (libse default: 24 ms) |
| `--balance-lines` | Balance line lengths |
| `--beautify-time-codes` | Beautify time codes |
| `--bridge-gaps:<ms>` | Bridge gaps shorter than N ms (extends previous end time) |
| `--convert-colors-to-dialog` | Convert colors to dialog |
| `--delete-first:<n>` | Delete first N entries |
| `--delete-last:<n>` | Delete last N entries |
| `--delete-contains:<word>` | Delete entries containing the given word |
| `--fix-common-errors` | Fix common subtitle errors (all 39 rules) |
| `--fix-common-errors-rules:<list>` | Run a subset of FCE rules (CSV; supports `all,-RuleId`) |
| `--fix-rtl-via-unicode-chars` | Fix RTL via Unicode characters |
| `--merge-same-texts` | Merge entries with same text |
| `--merge-same-time-codes` | Merge entries with same time codes |
| `--merge-short-lines` | Merge short lines |
| `--redo-casing` | Redo text casing |
| `--remove-formatting` | Remove formatting tags |
| `--remove-line-breaks` | Remove line breaks |
| `--remove-text-for-hi` | Remove text for hearing impaired |
| `--remove-unicode-control-chars` | Remove Unicode control characters |
| `--reverse-rtl-start-end` | Reverse RTL start/end |
| `--split-long-lines` | Split long lines |

```bash
# Common cleanup pass
seconv *.srt subrip --remove-text-for-hi --merge-same-texts --split-long-lines --overwrite
```

### FixCommonErrors rule selection

`--fix-common-errors` (no value) runs all 39 rules. Pass `--fix-common-errors-rules:<list>` to pick a subset — supplying that option implies `--fix-common-errors`.

```bash
seconv movie.srt subrip --fix-common-errors                                  # all rules
seconv movie.srt subrip --fix-common-errors-rules:FixCommas,FixMissingSpaces
seconv movie.srt subrip --fix-common-errors-rules:all,-FixDanishLetterI      # all except one
seconv list-fce-rules                                                        # show rule IDs
```

`FixCommonOcrErrors` is intentionally excluded — it requires UI-side spell-check and OCR-engine setup that seconv doesn't carry.

## Output format aliases

| Aliases | Format |
|---|---|
| `srt`, `subrip` | SubRip |
| `ass`, `assa` | Advanced Sub Station Alpha |
| `ssa` | Sub Station Alpha |
| `vtt`, `webvtt` | WebVTT |
| `smi`, `sami` | SAMI |
| `sbv` | YouTube SBV |
| `pac` | PAC (Screen Electronics) — binary |
| `unipac`, `pacunicode` | PAC Unicode |
| `ebu`, `ebustl`, `stl` | EBU STL — binary |
| `cavena`, `cavena890` | Cavena 890 — binary |
| `cheetahcaption` | CheetahCaption — binary |
| `capmakerplus` | CapMakerPlus — binary |
| `ayato` | Ayato — binary |
| `bluraysup`, `sup` | Blu-Ray sup — image |
| `vobsub` | VobSub — image |
| `bdnxml`, `bdn-xml` | BDN-XML — image (folder of PNGs + index.xml) |
| `dost`, `dostimage` | DOST/image |
| `fcpimage`, `fcp` | FCP/image |
| `dcinemainterop` | D-Cinema interop/png |
| `dcinemasmpte2014` | D-Cinema SMPTE 2014/png |
| `imageswithtimecode` | Images with time codes in file name |
| `plaintext`, `text`, `txt` | Plain text (HTML stripped) |
| `customtext`, `customtextformat` | Custom-templated text (requires `--custom-format`) |

Run `seconv formats` for the full catalog (380+ entries, including input-only formats like Matroska, MP4, and MCC).

## Exit codes

| Code | Meaning |
|---|---|
| `0` | Conversion succeeded for all matched files |
| `1` | Any error: validation failure, parse error, OCR engine missing, invalid `--settings` file, or one or more files failed to convert |

## Legacy syntax

- Old SE 4.x `/parameter:value` syntax is auto-translated to `--parameter:value`.
- Older smashed-together long options (`--inputfolder`, `--ocrengine`, `--FixCommonErrors`, …) are kept as hidden aliases for the new POSIX-style names (`--input-folder`, `--ocr-engine`, `--fix-common-errors`, …), so existing scripts keep working.

## More examples

### Bulk format conversion
```bash
seconv "*.srt" assa --input-folder:./input --output-folder:./output --overwrite
```

### MKV → SRT extraction
```bash
seconv movie.mkv subrip --overwrite                       # one SRT per language
seconv movie.mkv subrip --track-number:3 --overwrite      # only track 3
seconv movie.mkv subrip --forced-only --overwrite         # forced only
```

### Blu-Ray sup OCR with cleanup
```bash
seconv subs.sup subrip \
  --ocr-engine:tesseract \
  --ocr-language:eng \
  --remove-text-for-hi \
  --split-long-lines \
  --overwrite
```

### Round-trip via image-based output
```bash
seconv subs.srt bluraysup --resolution:1920x1080 --overwrite
seconv subs.srt bdnxml --overwrite
```

### Styled Blu-Ray sup (SE4 `/convert` parity)
```bash
# SE4: SubtitleEdit.exe /convert movie.srt blu-raysup /Resolution:3840x2160 (+ styling from Settings.xml)
seconv movie.srt bluraysup \
  --resolution:3840x2160 \
  --font-name:Arial --font-size:100 \
  --background-color:"#B4000000" \
  --overwrite
```

### EBU STL with reused header
```bash
seconv new.srt ebustl --ebu-header-file:original.stl --overwrite
```

### Time shift + frame-rate conversion
```bash
seconv subs.srt subrip --fps:24 --target-fps:25 --offset:500 --overwrite
```

### Plain text export
```bash
seconv movie.srt plaintext --overwrite
```

### Custom JSON output via template
```bash
seconv movie.srt customtext --custom-format:lines-template.xml --output-filename:movie.json --overwrite
```

## Architecture

`seconv` depends only on Subtitle Edit's core libraries — no Avalonia / GUI runtime:

```
src/
├── libse/        Core subtitle library (NuGet-shippable, netstandard2.1)
├── libuilogic/   Shared headless logic (BatchConverter pipeline, OCR matchers, image renderer)
├── seconv/       This CLI
└── ui/           Avalonia desktop UI (not referenced by seconv)
```

This means `seconv` can run on systems without a display (CI, headless servers, Docker) without bundling Avalonia or any X server.

## See also

- [Supported Formats](supported-formats.md) — full format catalog
- [Batch Convert](../features/batch-convert.md) — GUI equivalent
- [OCR](../features/ocr.md) — engine details and language packs
