# Supported Subtitle Formats

Subtitle Edit supports a wide range of subtitle formats for reading and writing. Below is a summary of the major categories. For the full machine-generated list of formats the build currently knows about, run `seconv formats` from the command line.

## Text-Based Formats

| Format | Extension(s) | Documentation |
|--------|--------------|---------------|
| SubRip | .srt | [Format Reference](subrip.md) |
| WebVTT (Web Video Text Tracks) | .vtt, .webvtt | [Format Reference](webvtt.md) |
| Advanced SubStation Alpha (ASSA) | .ass | [Format Reference](assa.md) |
| SubStation Alpha (SSA) | .ssa | [Format Reference](assa.md) |
| MicroDVD | .sub | |
| SAMI | .smi | |
| Timed Text (TTML) | .ttml, .xml, .dfxp | |
| iTunes Timed Text (iTT) | .itt | |
| EBU STL | .stl | |
| Spruce STL | .stl | |
| Scenarist Closed Captions (SCC) | .scc | |
| DVD Studio Pro | .txt | |
| Cavena 890 | .890 | |
| PAC | .pac | |
| Cheetah | .cap | |
| Avid DS | .txt | |
| JSON (various) | .json | |
| LRC (Lyrics) | .lrc | |
| and many more... | | |

> **See also:**
> - [SubRip Format Reference](subrip.md) — Complete guide to SRT format
> - [WebVTT Format Reference](webvtt.md) — Complete guide to WebVTT format (HTML5 standard)
> - [ASSA Format Reference](assa.md) — Complete guide to ASS/SSA format
> - [ASSA Override Tags Reference](assa-override-tags.md) — Complete list of ASS/SSA override tags for styling and animation

## Image-Based Formats

| Format | Extension(s) |
|--------|--------------|
| Blu-ray PGS (SUP) | .sup |
| VobSub (DVD) | .sub/.idx |
| BDN XML | .xml |

## Container Formats (with embedded subtitles)

| Format | Extension(s) |
|--------|--------------|
| Matroska (MKV/MKS) | .mkv, .mks |
| MP4 / MOV (text tracks) | .mp4, .m4v, .m4s, .3gp |
| Transport Stream (teletext, DVB-sub) | .ts, .m2ts, .mts |
| MacCaption | .mcc |

## Video Formats (for loading video)

| Format | Extension(s) |
|--------|--------------|
| Matroska | .mkv |
| MPEG-4 | .mp4 |
| Transport Stream | .ts |
| QuickTime | .mov |
| MPEG | .mpeg |
| Blu-ray Transport Stream | .m2ts |

> **Note:** Subtitle Edit ships with parsers/writers for 380+ subtitle formats, including text, binary, and image-based ones. The format is auto-detected when you open a file.
