# SeConv - Subtitle Edit Command Line Converter

A modern, headless command-line utility for batch converting subtitle files between formats.
Reuses Subtitle Edit's core libraries (libse, libuilogic) so seconv supports the same formats,
operations, and OCR engines as the desktop UI — without an Avalonia / GUI dependency.

## Features

- 380+ subtitle formats (text, binary, image-based)
- Container input: Matroska (.mkv/.mks), MP4, MCC, transport stream teletext
- OCR pipelines for image-based sources (Blu-Ray .sup, MKV PGS, DVB-sub)
- Five OCR engines: Tesseract subprocess, nOCR (built-in), BinaryOCR (built-in), Ollama (HTTP), PaddleOCR subprocess
- Image-based output: Blu-Ray sup, BDN-XML, DOST, FCP, D-Cinema interop / SMPTE 2014, images-with-time-code
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
| `--renumber:<n>` | Renumber paragraphs starting at `n` |

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

### Templates / replacements
| Option | Description |
|---|---|
| `--multiple-replace:<path.xml>` | SE MultipleSearchAndReplaceGroups XML |
| `--custom-format:<path.xml>` | SE CustomFormatItem XML (with `--format customtext`) |
| `--settings:<path.json>` | JSON settings file overriding libse defaults |
| `--profile:<name>` | Named overlay from settings file's `profiles` map |

### Verbosity
| Option | Description |
|---|---|
| `--quiet` / `-q` | Suppress per-file progress; only print the final summary |
| `--verbose` / `-v` | Print extra diagnostic information |
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

```bash
seconv movie.srt subrip --FixCommonErrors                              # all rules
seconv movie.srt subrip --FixCommonErrorsRules:FixCommas,FixMissingSpaces
seconv movie.srt subrip --FixCommonErrorsRules:all,-FixDanishLetterI   # all except one
seconv list-fce-rules                                                  # show rule IDs
```

`FixCommonOcrErrors` is intentionally excluded — it requires UI-side spell-check
and OCR engine setup that seconv doesn't carry.

## Output format aliases

```
srt / subrip                      ass / assa                      ssa
vtt / webvtt                      smi / sami                      sbv
pac                               unipac / pacunicode             ebu / ebustl / stl
cavena / cavena890                cheetahcaption                  capmakerplus
ayato
bluraysup / sup                   vobsub                          bdnxml / bdn-xml
dost                              fcpimage                        dcinemainterop
dcinemasmpte2014                  imageswithtimecode
plaintext / text / txt            customtext / customtextformat
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
