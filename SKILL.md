---
name: subtitle-edit
description: Subtitle Edit CLI — 格式批量转换、时间偏移、编码处理、帧率修正。使用 GUI 自带的 /convert 参数或独立 CLI seconv。
triggers:
  - subtitle edit
  - 字幕格式转换
  - srt 转 ass
  - ass 转 srt
  - 批量字幕
  - 字幕转码
  - batch convert subtitles
tags:
  - subtitle
  - conversion
  - media
  - srt
  - ass
  - vtt
---

# Subtitle Edit — Agent Skill

This skill helps AI agents work with Subtitle Edit's command-line interface for batch subtitle conversion and processing.

## Build & Development

```bash
# Build the full application (Windows)
dotnet build SubtitleEdit.sln

# The built executable will be at:
# src/ui/bin/Debug/net8.0-windows/SubtitleEdit.exe
```

## CLI Usage

```cmd
SubtitleEdit.exe /convert <pattern> <format-name> [parameters...]
```

> **Note:** `/list` may open the GUI instead of printing to console on some versions. Check the format dropdown in the GUI for the exact format names.

### Parameters

| Parameter | Description |
|---|---|
| `/convert "<pattern>" "<format>"` | Batch convert, pattern supports wildcards (e.g. `*.srt`) |
| `/list` | List all supported subtitle format names (may open GUI) |
| `/offset:hh:mm:ss.msec` | Time offset (positive = forward, negative = backward, e.g. `-00:00:02.000`) |
| `/fps:<float>` | Force frame rate (for image-based subtitle timing) |
| `/encoding:<name>` | Output encoding (e.g. `utf-8`, `ascii`). SE auto-detects source encoding but may guess wrong |
| `/inputfolder:"<path>"` | Input directory |
| `/outputfolder:"<path>"` | Output directory (default: overwrite source files) |
| `/multiplereplace` | Batch text replacement (requires pre-configured replace list in GUI) |
| `/batchconvert` | Open the batch convert GUI |

## Common Examples

```cmd
# srt → ass
SubtitleEdit.exe /convert "*.srt" AdvancedSubStationAlpha /outputfolder:"D:\output\"

# ass → srt (⚠️ loses styling: fonts, colors, position, effects)
SubtitleEdit.exe /convert "*.ass" SubRip

# srt → vtt (WebVTT)
SubtitleEdit.exe /convert "*.srt" WebVTT

# Time offset (shift all subtitles back 2 seconds)
SubtitleEdit.exe /convert "*.srt" SubRip /offset:-00:00:02.000

# Specify output encoding
SubtitleEdit.exe /convert "*.srt" SubRip /encoding:utf-8 /outputfolder:"D:\output\"

# Force frame rate (for image-based subtitles like VobSub)
SubtitleEdit.exe /convert "*.sub" SubRip /fps:23.976

# Batch text replacement
SubtitleEdit.exe /convert "*.srt" SubRip /multiplereplace
```

## Common Workflows

### Extract subtitles from MKV → Convert

```cmd
# 1. List subtitle tracks in MKV
mkvmerge -i "input.mkv"

# 2. Extract specific track (e.g. track ID 2)
mkvextract tracks "input.mkv" 2:"extracted.sup"

# 3. Convert with SE (text formats work directly; image formats need GUI OCR)
SubtitleEdit.exe /convert "extracted.sup" SubRip /outputfolder:"."
```

## Standalone CLI Tool (Cross-platform, Headless)

Alternative: `subtitleedit-cli` — a lightweight .NET 8 console app without GUI dependencies.

- Repository: <https://github.com/SubtitleEdit/subtitleedit-cli>
- .NET 8, cross-platform (Windows / Linux / macOS)
- Docker image available

```bash
# Build
git clone https://github.com/SubtitleEdit/subtitleedit-cli.git
cd subtitleedit-cli
dotnet build seconv.csproj

# Run (binary at src/se-cli/bin/Debug/net8.0/seconv)
./src/se-cli/bin/Debug/net8.0/seconv *.sub SubRip

# Docker
docker build -t seconv:1.0 -f docker/Dockerfile .
docker run --rm -it -v $(pwd)/subtitles:/subtitles seconv:1.0 sample.srt pac
```

## Configuration Files

```
%APPDATA%\Subtitle Edit\Settings.xml          — Global settings (OCR fixes, replace lists, etc.)
Dictionaries\<lang>_OCRFixReplaceList.xml      — OCR fix dictionary per language
```

## Troubleshooting

| Problem | Cause & Solution |
|---|---|
| `/convert` hangs / times out | SE shows a confirmation dialog without visible window. Ensure `outputfolder` is set and writable |
| Format name "not found" | Format names are case-sensitive. Check the exact name in the GUI format dropdown |
| Output file garbled | Source encoding is not UTF-8. Add `/encoding:utf-8` or `/encoding:shift-jis` (Japanese) |
| ass → srt loses all styling | ASS font/color/position/animation can't map to plain-text SRT — a format limitation |
| `/list` produces no output | Some versions open GUI instead. Check format names in the GUI dropdown |
| Multiple files overwrite each other | Check pattern and outputfolder for conflicts |

## Limitations ⚠️

| Feature | CLI Support |
|---|---|
| Text format conversion | ✅ |
| Time offset / frame rate | ✅ |
| Encoding | ✅ |
| Batch text replace | ✅ |
| OCR (image→text, VobSub/PGS) | ❌ GUI required |
| AI translation | ❌ GUI only |
| Complex editing (split/merge/timing) | ❌ |
| REST API / HTTP service | ❌ |
