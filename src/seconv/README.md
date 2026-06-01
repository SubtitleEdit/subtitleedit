# SeConv - Subtitle Edit Command Line Converter

A modern, headless command-line utility for batch converting subtitle files between formats.
Reuses Subtitle Edit's core libraries (libse, libuilogic) so seconv supports the same formats,
operations, and OCR engines as the desktop UI — without an Avalonia / GUI dependency.

## Features

- 380+ subtitle formats (text, binary, image-based)
- Container input: Matroska (.mkv/.mks), MP4, MCC, MXF, transport stream teletext
- OCR pipelines for image-based sources (Blu-Ray .sup, MKV PGS, DVB-sub)
- Five OCR engines: Tesseract subprocess, nOCR (built-in), BinaryOCR (built-in), Ollama (HTTP), PaddleOCR subprocess
- Image-based output: Blu-Ray sup, BDN-XML, DOST, FCP, D-Cinema interop / SMPTE 2014, images-with-time-code, WebVTT thumbnails
- Image-to-image conversion (preserve source bitmaps, no OCR): Blu-Ray .sup, VobSub .sub/.idx, MKV PGS, TS DVB-sub → any image output format
- Full operation pipeline: offset, fps change, renumber, adjust-duration, fix-common-errors,
  merge/split, balance, redo casing, RTL fixes, multiple-replace, custom-text format, plain text
- Cross-platform (Windows, Linux, macOS) — only requires .NET 10 runtime

## Installation

```bash
dotnet build src/seconv/SeConv.csproj
```

The executable is `seconv` / `seconv.exe`.

## Usage

```bash
seconv <pattern> <format> [options]
seconv <pattern> --format <name> [options]   # alternative syntax
```

### Quick examples

```bash
seconv *.srt sami                                              # SRT → SAMI
seconv movie.srt subrip --encoding:windows-1252                # encoding override
seconv movie.srt subrip --encoding:source                      # keep input's encoding on output
seconv *.srt subrip --input-encoding-fallback:windows-1250     # UTF-8 wins; CP1250 only if not UTF-8
seconv *.sub subrip --fps:25 --output-folder ./out             # frame-based → time-based

seconv movie.mkv subrip --track-number:3                       # extract MKV text track #3
seconv movie.sup subrip --ocr-engine:tesseract --ocr-language:eng  # OCR a Blu-Ray .sup
seconv movie.sup subrip --ocr-engine:nocr --ocr-db:Latin.nocr  # OCR via nOCR
seconv movie.sup subrip --ocr-engine:binaryocr --ocr-db:Latin.db # OCR via BinaryOCR
seconv movie.sup subrip --ocr-engine:ollama --ollama-model:llama3.2-vision

seconv subs.srt bluraysup --resolution:1920x1080               # render text → Blu-Ray sup
seconv subs.srt bdnxml --resolution:1920x1080                  # render text → BDN-XML

seconv movie.sup bdnxml                                        # preserve bitmaps: .sup → BDN-XML (no OCR)
seconv movie.sup vobsub                                        # preserve bitmaps: Blu-Ray → DVD
seconv movie.sub bdnxml                                        # preserve bitmaps: VobSub → BDN-XML (.idx auto-detected)
seconv movie.mkv bluraysup                                     # preserve bitmaps: MKV PGS track → .sup
seconv movie.ts dost                                           # preserve bitmaps: DVB-sub → DOST (one output per PID)

seconv subs.srt customtext --custom-format:my-template.xml     # custom template
seconv *.srt subrip --multiple-replace:rules.xml               # search-and-replace pass

seconv subs.srt subrip --offset:-2000 --renumber:1 --overwrite  # offset 2s back, renumber from 1
```

## Subcommands

```bash
seconv formats                # list all supported formats
seconv list-encodings         # list text encodings
seconv list-pac-codepages     # list PAC code pages
seconv list-ocr-engines       # list OCR engines + installation status
seconv list-fce-rules         # list FixCommonErrors rule IDs
seconv info <file>            # print format/encoding/duration/language for a file
seconv lint <pattern>         # validate subtitle(s); exit 1 if issues found
seconv --help                 # show help
```

### Inspect & validate

```bash
seconv info movie.srt                       # human-readable table
seconv info movie.srt --json                # machine-parseable

seconv lint *.srt                           # check overlaps, line lengths, tags, ...
seconv lint *.srt --json                    # CI-friendly: exit 1 on any issue
```

## Command Line Options

### File / I/O
| Option | Description |
|---|---|
| `--input-folder:<path>` | Input folder; relative patterns resolve against it |
| `--output-folder:<path>` | Output folder (default: input file's directory) |
| `--output-filename:<name>` | Output file name (single input only) |
| `--overwrite` | Overwrite existing files (default: rotate to `name_2.ext`, `_3.ext`, ...) |
| `--encoding:<name>` | Encoding name or codepage (defaults: auto-detect on input, UTF-8 BOM on output). Use `source` to keep the input file's detected encoding. |
| `--input-encoding-fallback:<name>` | Encoding to assume when input has no BOM and is not detected as UTF-8 (replaces the ANSI codepage heuristic). Ignored when `--encoding` is set. |

### Time / Frame
| Option | Description |
|---|---|
| `--offset:hh:mm:ss:ms` | Shift all timecodes (also accepts plain ms) |
| `--fps:<rate>` | Source frame rate |
| `--target-fps:<rate>` | Target frame rate (with `--fps`) |
| `--adjust-duration:<ms>` | Add/subtract milliseconds to each duration |
| `--change-speed:<percent>` | Scale all times by 100/percent (e.g. `125` = 1.25x faster) |
| `--apply-min-gap:<ms>` | Best-effort enforcement of a minimum gap between paragraphs by shortening the previous cue's end time. See note below. |
| `--renumber:<n>` | Renumber paragraphs starting at `n` |

#### `--apply-min-gap` vs `--FixCommonErrorsRules:FixOverlappingDisplayTimes`

Both touch boundary timings but solve different problems:

- `--apply-min-gap:<ms>` walks every pair of adjacent paragraphs and, when the
  gap is shorter than `<ms>`, **shortens the previous cue's end time** to open
  it up. It does **not** move the next cue's start. Pairs are also **skipped**
  when shortening would drop the previous cue below
  `General.SubtitleMinimumDisplayMilliseconds` — so this is best-effort, not a
  hard guarantee. Use it for delivery specs (e.g. broadcast standards that ask
  for 2 frames between cues) while accepting that minimum-display-duration
  collisions are left alone.
- `FixOverlappingDisplayTimes` is one rule inside Fix Common Errors and only
  resolves cases where paragraph end > next paragraph start (true overlap).
  It does *not* enforce a non-zero minimum gap once overlaps are gone.

In most batch pipelines `--apply-min-gap:<ms>` is the better choice; reach for
the FCE rule when you specifically want to keep tight 1 ms gaps the source
already has and only repair true overlaps.

### Format-specific
| Option | Description |
|---|---|
| `--resolution:<WxH>` | Video resolution for ASSA / image-based outputs (default 1920x1080) |
| `--assa-style-file:<file>` | Apply `[V4+ Styles]` from another ASSA file |
| `--pac-codepage:<page>` | PAC code page (name or 0-12) |
| `--ebu-header-file:<file>` | Use header from an existing STL file when writing EBU |

### Containers / Tracks
| Option | Description |
|---|---|
| `--track-number:<list>` | Comma-separated track numbers to keep |
| `--forced-only` | MKV: keep only forced tracks |
| `--teletext-only` | TS: skip DVB-sub OCR (teletext only) |
| `--teletext-only-page:<n>` | TS: extract only this teletext page |

### OCR
| Option | Description |
|---|---|
| `--ocr-engine:<engine>` | `tesseract` (default) \| `nocr` \| `binaryocr` \| `ollama` \| `paddle` |
| `--ocr-language:<lang>` | Tesseract: ISO 639-2 (`eng`, `deu`); Paddle: short (`en`); Ollama: human (`English`) |
| `--ocr-db:<path>` | OCR database file: `.nocr` for `nocr`, `.db` for `binaryocr` (required for both) |
| `--ollama-url:<url>` | Default `http://localhost:11434/api/chat` |
| `--ollama-model:<model>` | Default `llama3.2-vision` |

> **OCR database files are not bundled with `seconv`.** The `nocr` and `binaryocr` engines
> need a `.nocr` or `.db` file passed via `--ocr-db`. Sources:
> - If you have the desktop UI installed: `%AppData%\Subtitle Edit\OCR\` (Windows) or
>   `~/.config/Subtitle Edit/OCR/` (Linux/macOS).
> - From the repo: [`Ocr/Latin.nocr`](https://github.com/SubtitleEdit/subtitleedit/raw/main/Ocr/Latin.nocr)
>   and [`Ocr/Latin.db`](https://github.com/SubtitleEdit/subtitleedit/raw/main/Ocr/Latin.db).
> - Other languages: download from the SE UI (Tools → "OCR with nOCR" / BinaryOCR → download).

### Image-to-image conversion (no OCR)

When **both** the input and target are image-based subtitle formats, seconv passes the
source bitmaps straight through to the output handler instead of OCR'ing them to text
and re-rasterising at the CLI's default font. Routing is automatic — no flag needed:

| Input | Detection | Outputs |
|---|---|---|
| `.sup` | extension | any image format |
| `.sub` (VobSub) | `.idx` companion next to it | any image format |
| `.mkv` / `.mks` | `S_HDMV/PGS` track present | any image format (one output per track) |
| `.ts` / `.m2ts` / `.mts` | DVB-sub PID present | any image format (one output per PID) |

The same input with a text target (`subrip`, `ass`, ...) still routes through OCR.
Source video dimensions (e.g. 1920x1080 for Blu-Ray) are preserved automatically and
take precedence over `--resolution`.

```bash
seconv movie.sup bdnxml                                        # Blu-Ray → BDN-XML for NLE re-authoring
seconv movie.sup vobsub                                        # Blu-Ray → DVD
seconv movie.sub bluraysup                                     # DVD → Blu-Ray (.idx auto-detected)
seconv movie.mkv dost                                          # MKV PGS → DOST (per track)
seconv movie.ts fcpimage                                       # DVB-sub → FCP image XML (per PID)
```

### Templates / replacements
| Option | Description |
|---|---|
| `--multiple-replace:<path.xml>` | SE MultipleSearchAndReplaceGroups XML |
| `--custom-format:<path.xml>` | SE CustomFormatItem XML (with `--format customtext`) |
| `--settings:<path.json>` | JSON settings file overriding libse defaults (seconv-specific schema — **not** the SE 5 GUI's `Settings.json`) |
| `--profile:<name>` | Named overlay from settings file's `profiles` map |

> ⚠️ **`--settings:` is not the SE 5 GUI's `Settings.json`.** seconv accepts a
> small, libse-shaped schema with three top-level keys: `general`, `tools`, and
> `removeTextForHearingImpaired`. Property names follow the **libse** spelling,
> e.g. `RemoveIfAllUppercase` — *not* the GUI's `IsRemoveTextUppercaseLineOn`.
> The GUI's `Settings.json` is a much larger, GUI-specific dump and can't be
> consumed directly.
>
> Minimal example: removing speaker labels on all-uppercase lines.
>
> ```json
> {
>   "removeTextForHearingImpaired": {
>     "RemoveIfAllUppercase": true
>   }
> }
> ```
>
> ```bash
> seconv movie.srt subrip --settings:./hi.json --remove-text-for-hi
> ```
>
> The full list of supported keys lives in `src/seconv/Core/SeConvSettings.cs`.

### Verbosity
| Option | Description |
|---|---|
| `--quiet` / `-q` | Suppress per-file progress; only print the final summary |
| `--verbose` / `-v` | Print extra diagnostic information (incl. full `Exception.ToString()` with stack traces on errors; without it, the message chain is still walked so wrapper exceptions like `TypeInitializationException` surface their root cause) |
| `--json` | Emit per-file results as JSON to stdout (suppresses Spectre output) |

## Operations (boolean flags)

Applied in a fixed, sensible order regardless of CLI order:

`--ApplyDurationLimits` `--BalanceLines` `--BeautifyTimeCodes` `--ConvertColorsToDialog`
`--DeleteFirst:<n>` `--DeleteLast:<n>` `--DeleteContains:<word>` `--FixCommonErrors`
`--FixRtlViaUnicodeChars` `--MergeSameTexts` `--MergeSameTimeCodes` `--MergeShortLines`
`--RedoCasing` `--RemoveFormatting` `--RemoveLineBreaks` `--RemoveTextForHI`
`--RemoveUnicodeControlChars` `--ReverseRtlStartEnd` `--SplitLongLines`

### FixCommonErrors rule selection

`--FixCommonErrors` (no value) runs all 38 rules. Pass `--FixCommonErrorsRules:<list>`
to pick a subset — supplying the option implies `--FixCommonErrors`.

A handful of rules are **language-conditional** and only run in the default
"all rules" pass when the auto-detected language matches. The GUI's Fix Common
Errors window has the same gating:

- `FixAloneLowercaseIToUppercaseI` — only on `en`
- `FixDanishLetterI` — only on `da`
- `FixSpanishInvertedQuestionAndExclamationMarks` — only on `es`
- `FixTurkishAnsiToUnicode` — only on `tr`

You can still opt in on any content by naming the rule explicitly in
`--FixCommonErrorsRules` (e.g. `--FixCommonErrorsRules:FixSpanishInvertedQuestionAndExclamationMarks`).

```bash
seconv movie.srt subrip --FixCommonErrors                              # all rules
seconv movie.srt subrip --FixCommonErrorsRules:FixCommas,FixMissingSpaces
seconv movie.srt subrip --FixCommonErrorsRules:all,-FixDanishLetterI   # all except one
seconv list-fce-rules                                                  # show rule IDs
```

`FixCommonOcrErrors` is intentionally excluded — it requires UI-side spell-check
and OCR engine setup that seconv doesn't carry.

## Output format aliases

All aliases below are case-insensitive and accepted by `--format` / the positional
argument. Only the spellings shown in the table are recognised — variants that
aren't listed (e.g. `plain-text`, `custom-text`) won't resolve.

```
srt / subrip                       ass / assa                       ssa
vtt / webvtt                       smi / sami                       sbv
pac                                unipac / pacunicode              ebu / ebustl / stl
cavena / cavena890                 cheetah / cheetahcaption         capmaker / capmakerplus
ayato                              vobsub                           bluraysup / sup
bdnxml / bdn-xml                   dost / dostimage                 fcp / fcpimage
dcinemainterop / dcinema-interop   dcinemasmpte2014 / dcinema-smpte
imageswithtimecode / imagesintc    webvttthumbnail / webvtt-thumbnail / vttthumb
plaintext / text / txt             customtext / customtextformat
```

## Exit codes

- `0` – conversion succeeded for all matched files
- `1` – any error: validation failure, parse error, or one or more files failed to convert

## Project Structure

```
src/
├── libse/        # core subtitle library (NuGet-shippable, netstandard2.1)
├── libuilogic/   # shared headless logic (BatchConverter pipeline, OCR matchers, image renderer)
├── seconv/       # this CLI
└── ui/           # Avalonia desktop UI (not referenced by seconv)
```

`seconv` references only `libse` and `libuilogic` — no Avalonia/MVVM transitive deps.

## Legacy syntax

Old SE 4.x `/parameter:value` syntax is auto-translated to `--parameter:value`.

## License

Part of the Subtitle Edit project.
