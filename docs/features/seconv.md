# seconv — Command Line Converter

`seconv` is Subtitle Edit's headless command-line converter. It supports the same formats, operations, and OCR engines as the desktop app — without any GUI dependency — and is suitable for scripts, CI pipelines, and bulk conversion.

```bash
seconv *.srt webvtt
seconv movie.srt subrip --encoding:source --FixCommonErrors
seconv movie.mkv subrip --track-number:3
seconv movie.sup subrip --ocr-engine:tesseract --ocr-language:eng
seconv movie.sub subrip --ocr-engine:binaryocr --ocr-db:Latin.db   # VobSub (.idx auto-detected)
seconv movie.sub subrip --ocr-engine:tesseract --no-vobsub-isolate-colors  # OCR raw palette (isolation is on by default)
seconv movie.sup subrip --time-codes-only
```

For full usage, options, OCR setup, operations pipeline, examples, and exit codes, see the canonical reference:

➡️ **[Command Line (seconv) — full reference](../reference/command-line.md)**

## See also

- [Batch Convert](batch-convert.md) — GUI equivalent
- [OCR](ocr.md) — engine details and language packs
