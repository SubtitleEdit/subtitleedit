# SeConv — Subtitle Edit Command Line Converter

A modern, headless command-line utility for batch converting subtitle files between formats.
It reuses Subtitle Edit's core libraries (libse, libuilogic), so `seconv` supports the same
formats, operations, and OCR engines as the desktop app — without any Avalonia / GUI dependency.
Cross-platform (Windows, Linux, macOS); only needs the .NET 10 runtime.

## What it does

- 380+ subtitle formats (text, binary, image-based)
- Container input: Matroska (.mkv/.mks), MP4, MCC, MXF, transport stream teletext
- OCR for image-based sources (Blu-ray .sup, VobSub .sub/.idx, MKV PGS/VobSub, MP4 VobSub, TS DVB-sub)
  via five engines: Tesseract, nOCR, BinaryOCR, Ollama, PaddleOCR — or `--time-codes-only` to skip OCR
- Image-based output and image-to-image conversion (preserve source bitmaps, no OCR)
- Full operation pipeline: offset, fps change, renumber, adjust-duration, fix-common-errors,
  merge/split, balance, redo casing, RTL fixes, multiple-replace, custom-text format, plain text

## Build

```bash
dotnet build src/seconv/SeConv.csproj
```

The executable is `seconv` / `seconv.exe`.

## Quick start

```bash
seconv <pattern> <format> [options]
seconv <pattern> --format <name> [options]   # alternative syntax

seconv *.srt webvtt                                               # SRT → WebVTT
seconv movie.srt subrip --encoding:source --fix-common-errors    # keep encoding, clean up
seconv movie.mkv subrip --track-number:3                         # extract MKV text track #3
seconv movie.sup subrip --ocr-engine:tesseract --ocr-language:eng # OCR a Blu-ray .sup
seconv movie.sup subrip --time-codes-only                        # timing only, no OCR
seconv subs.srt bluraysup --resolution:1920x1080                 # render text → Blu-ray sup
seconv dump-settings > my.json                                   # starter --settings file (libse defaults)
```

Run `seconv` with no arguments for built-in help, or `seconv formats` to list every format.

## Full reference

➡ **[Command Line (seconv) — full reference](../../docs/reference/command-line.md)**

The canonical reference documents every option, OCR setup, the operations pipeline, output-format
aliases, templates/replacements, exit codes, and more examples. To avoid drift, detailed docs live
there (and at [docs/features/seconv.md](../../docs/features/seconv.md)) rather than being duplicated here.
