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
| EBU-TT (Tech 3350) | .xml | |
| Spruce STL | .stl | |
| Scenarist Closed Captions (SCC) | .scc | |
| DVD Studio Pro | .stl | |
| Cavena 890 | .890 | |
| CANVASs SSTG1 project (SSTG1Pro/Lite, import only) | .sdb | |
| DVB Teletext (Manzanita; import - export via File → Export → DVB teletext (Manzanita)) | .dvbttx | |
| DVD Junior SPC | .spc | |
| Sonic DVD Producer | .txt | |
| PAC | .pac | |
| Cheetah | .cap | |
| Avid DVD, Avid Caption, Avid Caption Drop Frame, Avid Loc Markers | .txt | |
| Avid STL | .stl | |
| JSON (various) | .json | |
| Wistia json | .json | |
| Csv Excel | .csv | |
| YouTube timed text srv3 | .xml, .ytt, .srv3 | |
| LRC (Lyrics) | .lrc | |
| and many more... | | |

> **See also:**
> - [SubRip Format Reference](subrip.md) — Complete guide to SRT format
> - [WebVTT Format Reference](webvtt.md) — Complete guide to WebVTT format (HTML5 standard)
> - [ASSA Format Reference](assa.md) — Complete guide to ASS/SSA format
> - [ASSA Override Tags Reference](assa-override-tags.md) — Complete list of ASS/SSA override tags for styling and animation

## Editing / NLE Exchange Formats

Formats used to move captions and markers between Subtitle Edit and video editors.

| Format | Extension(s) |
|--------|--------------|
| Final Cut Pro Xml Captions | .fcpxml |
| Final Cut Pro Xml (1.3 – 1.5, X, text and marker variants) | .xml, .fcpxml |
| DaVinci Resolve Marker EDL | .edl |
| Csv DaVinci | .csv |
| EDL | .edl |
| Adobe Premiere Markers | .csv |
| Adobe Premiere PrProj Xml | .xml |
| Adobe Encore (tabs) | .txt |
| Audacity / Tenacity labels | .txt |

## Spreadsheets

Import only — a spreadsheet with a header row naming start/end/text columns is read directly, and any layout can be mapped by hand in *File → Import → CSV/XLSX/ODS with custom columns*. See [Import Spreadsheets](../features/import-csv-xlsx.md).

| Format | Extension(s) |
|--------|--------------|
| Excel workbook (Office Open XML) | .xlsx |
| OpenDocument spreadsheet | .ods |
| Delimited text (comma, semicolon, tab, pipe) | .csv, .tsv, .txt |

## Image-Based Formats

| Format | Extension(s) |
|--------|--------------|
| Blu-ray PGS (SUP) | .sup |
| VobSub (DVD) | .sub/.idx |
| BDN XML | .xml |
| Timed Text Base64 Image (SMPTE-TT bitmap) | .xml |
| Timed Text Image | .xml |
| BDN XML 8-bit (palette-indexed PNGs, export) | .xml |
| DOST | .dost |
| Final Cut Pro Image (FCP/image) | .xml |
| SPU Image | .xml |
| D-Cinema interop/png, D-Cinema SMPTE 2014/png (export) | .xml |
| Images with time codes in file name (export) | image files |
| WebVTT Thumbnail (sprite sheet) | .vtt |

## Container Formats (with embedded subtitles)

| Format | Extension(s) |
|--------|--------------|
| Matroska (MKV/MKS/WebM) | .mkv, .mks, .webm |
| MP4 / MOV (text tracks, including fragmented MP4/DASH — wvtt, stpp, tx3g) | .mp4, .m4v, .m4s, .3gp, .mov |
| Transport Stream (teletext, DVB-sub, ARIB STD-B24 captions) | .ts, .m2ts, .mts |
| AVI (XSUB) | .avi, .divx |
| MacCaption | .mcc |
| MXF (timed-text essences) | .mxf |

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

## Format Notes

- **Scenarist Closed Captions (SCC):** font colors are written as CEA-608 mid-row codes (a mid-row code displays as a space, so it costs one cell). A line holds at most 32 characters and a caption at most 4 rows; with *Warn on save when lines exceed the format's limits* enabled (Options → Settings) a "Format limits exceeded" dialog lists the offending lines before the file is written. Import timing is frame-accurate: CEA-608 carries one byte pair per frame, so a display code takes effect at the line's time code plus its position in the line.
- **EBU STL / DVB teletext:** the video preview draws the text the way a teletext decoder would, with the box behind the text and double-height rows. While EBU STL is the active format the time codes are shown frame-based (HH:MM:SS:FF) using the frame rate from the STL header; the display reverts when another format is chosen.
- **Lambda Cap:** ruby (furigana), emphasis marks and tate-chu-yoko are decoded to the same markup as "Netflix IMSC 1.1 Japanese", so the video preview draws them instead of showing the control codes as text.
- **EBU-TT-D / IMSC Rosetta:** `<font color>` tags are written as referential styles. Rosetta allows only its eight fixed foreground styles, so a color is snapped to the nearest of black, red, yellow, green, cyan, blue, magenta and white; EBU-TT-D uses the same eight teletext colors and reads them back by name, so a round trip keeps `<font color="yellow">`.
