# SeConv - Subtitle Edit Command Line Converter

A modern, headless command-line utility for batch converting subtitle files between formats.
Reuses Subtitle Edit's core libraries (libse, libuilogic) so seconv supports the same formats,
operations, and OCR engines as the desktop UI — without an Avalonia / GUI dependency.

## Features

- 300+ subtitle formats (text, binary, image-based)
- Container input: Matroska (.mkv/.mks), MP4, MCC, transport stream teletext
- OCR pipelines for image-based sources (Blu-Ray .sup, MKV PGS, DVB-sub)
- Four OCR engines: Tesseract subprocess, nOCR (built-in), Ollama (HTTP), PaddleOCR subprocess
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
seconv *.sub subrip --fps:25 --outputfolder ./out              # frame-based → time-based

seconv movie.mkv subrip --track-number:3                       # extract MKV text track #3
seconv movie.sup subrip --ocrengine:tesseract --ocrlanguage:eng  # OCR a Blu-Ray .sup
seconv movie.sup subrip --ocrengine:nocr --ocrdb:Latin.nocr    # OCR via nOCR
seconv movie.sup subrip --ocrengine:ollama --ollama-model:llama3.2-vision

seconv subs.srt bluraysup --resolution:1920x1080               # render text → Blu-Ray sup
seconv subs.srt bdnxml --resolution:1920x1080                  # render text → BDN-XML

seconv subs.srt customtext --customformat:my-template.xml      # custom template
seconv *.srt subrip --multiplereplace:rules.xml                # search-and-replace pass

seconv subs.srt subrip --offset:-2000 --renumber:1 --overwrite  # offset 2s back, renumber from 1
```

## Subcommands

```bash
seconv formats                # list all supported formats
seconv list-encodings         # list text encodings
seconv list-pac-codepages     # list PAC code pages
seconv list-ocr-engines       # list OCR engines + installation status
seconv --help                 # show help
```

## Command Line Options

### File / I/O
| Option | Description |
|---|---|
| `--inputfolder:<path>` | Input folder; relative patterns resolve against it |
| `--outputfolder:<path>` | Output folder (default: input file's directory) |
| `--outputfilename:<name>` | Output file name (single input only) |
| `--overwrite` | Overwrite existing files (default: rotate to `name_2.ext`, `_3.ext`, ...) |
| `--encoding:<name>` | Encoding name or codepage (defaults: auto-detect on input, UTF-8 BOM on output) |

### Time / Frame
| Option | Description |
|---|---|
| `--offset:hh:mm:ss:ms` | Shift all timecodes (also accepts plain ms) |
| `--fps:<rate>` | Source frame rate |
| `--targetfps:<rate>` | Target frame rate (with `--fps`) |
| `--adjustduration:<ms>` | Add/subtract milliseconds to each duration |
| `--renumber:<n>` | Renumber paragraphs starting at `n` |

### Format-specific
| Option | Description |
|---|---|
| `--resolution:<WxH>` | Video resolution for ASSA / image-based outputs (default 1920x1080) |
| `--assa-style-file:<file>` | Apply `[V4+ Styles]` from another ASSA file |
| `--pac-codepage:<page>` | PAC code page (name or 0-12) |
| `--ebuheaderfile:<file>` | Use header from an existing STL file when writing EBU |

### Containers / Tracks
| Option | Description |
|---|---|
| `--track-number:<list>` | Comma-separated track numbers to keep |
| `--forcedonly` | MKV: keep only forced tracks |
| `--teletextonly` | TS: skip DVB-sub OCR (teletext only) |
| `--teletextonlypage:<n>` | TS: extract only this teletext page |

### OCR
| Option | Description |
|---|---|
| `--ocrengine:<engine>` | `tesseract` (default) \| `nocr` \| `ollama` \| `paddle` |
| `--ocrlanguage:<lang>` | Tesseract: ISO 639-2 (`eng`, `deu`); Paddle: short (`en`); Ollama: human (`English`) |
| `--ocrdb:<path.nocr>` | nOCR database file (required for `--ocrengine=nocr`) |
| `--ollama-url:<url>` | Default `http://localhost:11434/api/chat` |
| `--ollama-model:<model>` | Default `llama3.2-vision` |

### Templates / replacements
| Option | Description |
|---|---|
| `--multiplereplace:<path.xml>` | SE MultipleSearchAndReplaceGroups XML |
| `--customformat:<path.xml>` | SE CustomFormatItem XML (with `--format customtext`) |
| `--settings:<path.json>` | JSON settings file overriding libse defaults |
| `--profile:<name>` | Named overlay from settings file's `profiles` map |

### Verbosity
| Option | Description |
|---|---|
| `--quiet` / `-q` | Suppress per-file progress; only print the final summary |
| `--verbose` / `-v` | Print extra diagnostic information |

## Operations (boolean flags)

Applied in a fixed, sensible order regardless of CLI order:

`--ApplyDurationLimits` `--BalanceLines` `--BeautifyTimeCodes` `--ConvertColorsToDialog`
`--DeleteFirst:<n>` `--DeleteLast:<n>` `--DeleteContains:<word>` `--FixCommonErrors`
`--FixRtlViaUnicodeChars` `--MergeSameTexts` `--MergeSameTimeCodes` `--MergeShortLines`
`--RedoCasing` `--RemoveFormatting` `--RemoveLineBreaks` `--RemoveTextForHI`
`--RemoveUnicodeControlChars` `--ReverseRtlStartEnd` `--SplitLongLines`

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
