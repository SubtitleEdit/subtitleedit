# Command Line (seconv)

Subtitle Edit ships with a headless command-line converter called **seconv**. It supports
the same subtitle formats, operations, and OCR engines as the desktop app — without
loading the GUI — and is suitable for scripts, CI pipelines, and batch workflows.

The executable is named `seconv` (or `seconv.exe` on Windows). Pre-built binaries are
distributed alongside Subtitle Edit; you can also build from source:

```bash
dotnet build src/seconv/SeConv.csproj -c Release
```

## Quick start

```bash
# Basic conversion
seconv input.srt subrip
seconv *.srt webvtt
seconv input.srt --format AdvancedSubStationAlpha

# All matched files; recurse via --inputfolder
seconv "*.srt" subrip --inputfolder C:\input --outputfolder C:\output --overwrite

# Show the catalog of formats / encodings / OCR engines
seconv formats
seconv list-encodings
seconv list-pac-codepages
seconv list-ocr-engines
seconv --help
```

## Syntax

```
seconv <pattern> [<format>] [options]
seconv <pattern> --format <name> [options]    # equivalent
```

The format may be passed as the second positional arg (`seconv *.srt sami`)
or via `--format <name>`. Multiple input patterns can be passed as separate
quoted arguments or comma-separated:

```bash
seconv "file1.srt" "file2.srt" subrip --overwrite
seconv "*.srt,*.ass" subrip --inputfolder ./in
```

Legacy SE 4.x syntax (`/parameter:value`) is auto-translated to `--parameter:value`.

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
| `customtext`, `customtextformat` | Custom-templated text (requires `--customformat`) |

Run `seconv formats` to see the full catalog of 380+ supported formats including
input-only formats (Matroska, MP4, MCC, etc.).

## Container inputs

`seconv` automatically extracts subtitle tracks from container files:

| Extension | Sources |
|---|---|
| `.mkv`, `.mks` | Matroska text tracks (S_TEXT/UTF8, SSA, ASS, HDMV/TEXTST) and image tracks (S_HDMV/PGS via OCR) |
| `.mp4`, `.m4v`, `.m4s`, `.3gp` | MP4 text tracks and WebVTT VTTC |
| `.mcc` | MacCaption 1.0 |
| `.ts`, `.m2ts`, `.mts` | Transport stream — teletext (no OCR) and DVB-sub (via OCR) |
| `.sup` | Blu-Ray sup (via OCR) |

When a container has multiple usable tracks, one output file is written per track
with the track's language code as a suffix:

```
movie.mkv → movie.eng.srt
            movie.deu.srt
            movie.fra.srt
```

If two tracks share a language, the track number is added: `movie.#3.eng.srt`.

## File / I/O options

| Option | Description |
|---|---|
| `--inputfolder <path>` | Input folder. Relative patterns are resolved against it. |
| `--outputfolder <path>` | Output folder. Default: each input file's directory. |
| `--outputfilename <name>` | Output file name (only with a single input file). |
| `--overwrite` | Overwrite existing files. Without this, seconv rotates to `name_2.ext`, `name_3.ext`, etc. |
| `--encoding <name>` | Encoding for input/output. Default: auto-detect on input, UTF-8 BOM on output. Special values: `utf-8`, `utf-8-no-bom`, or a code page number. |

## Time / frame options

| Option | Description |
|---|---|
| `--offset hh:mm:ss:ms` | Shift all timecodes. Also accepts plain milliseconds (signed: `-1500`, `+1500`). |
| `--fps <rate>` | Source frame rate (used for frame-based input formats). |
| `--targetfps <rate>` | Target frame rate. Combined with `--fps`, calls `Subtitle.ChangeFrameRate`. |
| `--adjustduration <ms>` | Add/subtract milliseconds to each paragraph's duration. |
| `--renumber <n>` | Renumber paragraphs starting at `n`. |

## Format-specific options

| Option | Applies to | Description |
|---|---|---|
| `--resolution WxH` | ASSA, image-based | Sets `PlayResX`/`PlayResY` for ASSA; sets canvas for image outputs (default `1920x1080`). |
| `--assa-style-file <path>` | ASSA | Apply `[V4+ Styles]` block from another `.ass` file. |
| `--pac-codepage <page>` | PAC | Code page name (`Latin`, `Greek`, `Hebrew`, …) or numeric (0–12). See `seconv list-pac-codepages`. |
| `--ebuheaderfile <path>` | EBU STL | Reuse the GSI header block from an existing `.stl` file. |

## Container / track options

| Option | Description |
|---|---|
| `--track-number <list>` | Comma-separated track IDs to keep when extracting from a container. |
| `--forcedonly` | Matroska: only extract forced tracks. |
| `--teletextonly` | Transport stream: skip DVB-sub OCR (teletext only). |
| `--teletextonlypage <n>` | Transport stream: extract only this teletext page. |

## OCR options

`seconv` can OCR image-based subtitles (Blu-Ray sup, MKV PGS, transport stream DVB-sub)
into text. Four engines are supported:

| `--ocrengine` | Type | Setup |
|---|---|---|
| `tesseract` (default) | Subprocess | Install Tesseract (e.g. `apt install tesseract-ocr`, `brew install tesseract`, or the Windows installer); ensure it is on `PATH`. Pass `--ocrlanguage` as ISO 639-2 (`eng`, `deu`, `spa`, …). |
| `nocr` | In-process | Built-in nOCR matcher. Required: `--ocrdb=<path-to-Latin.nocr>` (find these under `%AppData%\Subtitle Edit\Ocr\` or download via the SE UI). |
| `ollama` | HTTP | Local Ollama server with a vision-capable model (e.g. `llama3.2-vision`, `qwen2.5vl`). Configure via `--ollama-url` (default `http://localhost:11434/api/chat`) and `--ollama-model` (default `llama3.2-vision`). Pass `--ocrlanguage` as a human name like `English`. |
| `paddle`, `paddleocr` | Subprocess | Install via `pip install paddleocr`; ensure the `paddleocr` binary is on `PATH`. Pass `--ocrlanguage` as a short code (`en`, `de`, …). |

Run `seconv list-ocr-engines` for the per-engine installation status table.

```bash
# OCR a Blu-Ray sup using Tesseract
seconv movie.sup subrip --ocrengine tesseract --ocrlanguage eng

# Same source via nOCR (no external dependency)
seconv movie.sup subrip --ocrengine nocr --ocrdb "C:\Users\me\AppData\Roaming\Subtitle Edit\Ocr\Latin.nocr"

# MKV with image (PGS) tracks — OCR runs automatically
seconv movie.mkv subrip --ocrengine tesseract --ocrlanguage eng

# Transport-stream teletext extraction (no OCR needed)
seconv broadcast.ts subrip
```

## Templates and replacements

| Option | Description |
|---|---|
| `--multiplereplace <path.xml>` | Subtitle Edit *MultipleSearchAndReplaceGroups* XML, applied per paragraph after operations. Supports `Normal` (case-insensitive), `CaseSensitive`, and `RegularExpression` rules. |
| `--customformat <path.xml>` | Subtitle Edit *CustomFormatItem* XML. Use with `--format customtext` to render via the template. |
| `--settings <path.json>` | JSON file overlaying `Configuration.Settings` (general / tools / removeTextForHearingImpaired). Optional `profiles` map for named overlays. |
| `--profile <name>` | Selects a named overlay from the settings file's `profiles` map. Requires `--settings`. |

### Example: multiple-replace XML

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
seconv *.srt subrip --multiplereplace fixes.xml
```

### Example: custom text format XML

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
seconv subs.srt customtext --customformat lines.xml
```

Available template tokens: `{title}`, `{number}`, `{start}`, `{end}`, `{duration}`,
`{gap}`, `{text}`, `{text-line-1}`, `{text-line-2}`, `{actor}`, `{cps-comma}`,
`{cps-period}`, `{text-length}`, `{bookmark}`, `{media-file-name}`, `{media-file-name-full}`,
`{#lines}`, `{#total-words}`, `{#total-characters}`, `{tab}`. Time code tokens accept
`hh`/`h`, `mm`/`m`, `ss`/`s`, and `zzz`/`zz`/`z` for milliseconds (or `zzzzzzz...` for
total milliseconds without breakdown).

### Example: settings JSON

```json
{
  "general": {
    "subtitleLineMaximumLength": 43,
    "subtitleMaximumDisplayMilliseconds": 8000
  },
  "removeTextForHearingImpaired": {
    "removeTextBeforeColon": true,
    "removeInterjections": false
  },
  "profiles": {
    "broadcast": {
      "general": { "subtitleMaximumDisplayMilliseconds": 6000 },
      "removeTextForHearingImpaired": { "removeInterjections": true }
    }
  }
}
```

```bash
seconv *.srt subrip --settings my.json --profile broadcast --RemoveTextForHI
```

## Operations

Operations run after the structural transforms (offset, fps, renumber, adjust-duration)
in a fixed, sensible order regardless of where they appear on the command line:

`--ApplyDurationLimits` `--BalanceLines` `--BeautifyTimeCodes` `--ConvertColorsToDialog`
`--DeleteFirst <n>` `--DeleteLast <n>` `--DeleteContains <word>` `--FixCommonErrors`
`--FixRtlViaUnicodeChars` `--MergeSameTexts` `--MergeSameTimeCodes` `--MergeShortLines`
`--RedoCasing` `--RemoveFormatting` `--RemoveLineBreaks` `--RemoveTextForHI`
`--RemoveUnicodeControlChars` `--ReverseRtlStartEnd` `--SplitLongLines`

```bash
# Common cleanup pass
seconv *.srt subrip --RemoveTextForHI --MergeSameTexts --SplitLongLines --overwrite
```

## Verbosity

| Option | Description |
|---|---|
| `--quiet`, `-q` | Suppress per-file progress and the parameters table; only print the final summary. |
| `--verbose`, `-v` | Reserved for diagnostic output (currently no-op). |

## Subcommands

| Subcommand | Output |
|---|---|
| `seconv formats` | Table of all 380+ formats with extension and type (text / binary / image). |
| `seconv list-encodings` | All .NET text encodings with code page, name, description. |
| `seconv list-pac-codepages` | PAC code pages (0–12) with names and aliases. |
| `seconv list-ocr-engines` | OCR engines with installation status (✓ installed / not on PATH). |

## Exit codes

| Code | Meaning |
|---|---|
| `0` | Conversion succeeded for all matched files. |
| `1` | Runtime error: file load/save failure, OCR engine missing, invalid `--settings` file, etc. |
| `127` | Spectre.Console.Cli usage error: missing required argument, unknown flag. |

## Examples

### Bulk format conversion
```bash
# Convert all SRTs in a folder to ASSA, preserving timestamps
seconv "*.srt" assa --inputfolder ./input --outputfolder ./output --overwrite
```

### MKV → SRT extraction
```bash
# Extract all text tracks (one SRT per language)
seconv movie.mkv subrip --overwrite

# Extract only the German track
seconv movie.mkv subrip --track-number 3 --overwrite

# Extract forced subtitles only
seconv movie.mkv subrip --forcedonly --overwrite
```

### Blu-Ray sup OCR with cleanup
```bash
seconv subs.sup subrip \
  --ocrengine tesseract \
  --ocrlanguage eng \
  --RemoveTextForHI \
  --SplitLongLines \
  --overwrite
```

### Round-trip via image-based output
```bash
# SRT → Blu-Ray sup (renders text → bitmaps)
seconv subs.srt bluraysup --resolution 1920x1080 --overwrite

# SRT → BDN-XML (folder of PNGs + index.xml)
seconv subs.srt bdnxml --overwrite
```

### EBU STL with reused header
```bash
# Convert to EBU STL while preserving the original GSI metadata
seconv new.srt ebustl --ebuheaderfile original.stl --overwrite
```

### Time shift + frame-rate conversion
```bash
# 24 fps → 25 fps with a 500 ms positive offset
seconv subs.srt subrip --fps 24 --targetfps 25 --offset 500 --overwrite
```

### Plain text export
```bash
seconv movie.srt plaintext --overwrite
```

### Custom JSON output via template
```bash
seconv movie.srt customtext --customformat lines-template.xml --outputfilename movie.json --overwrite
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

This means `seconv` can run on systems without a display (CI, headless servers, Docker)
without bundling Avalonia or any X server.

## See also

- [Supported Formats](supported-formats.md) — full format catalog
- [Batch Convert](../features/batch-convert.md) — GUI equivalent
- [OCR](../features/ocr.md) — engine details and language packs
