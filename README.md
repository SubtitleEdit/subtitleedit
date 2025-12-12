# Subtitle Edit

the subtitle editor :)

[![GitHub version](https://img.shields.io/github/release/SubtitleEdit/subtitleedit.svg)](https://github.com/SubtitleEdit/subtitleedit)
[![Build status](https://img.shields.io/appveyor/ci/SubtitleEdit/subtitleedit/main.svg)](https://ci.appveyor.com/project/SubtitleEdit/subtitleedit/branch/main)
![SE number of downloads](https://img.shields.io/github/downloads/subtitleedit/subtitleedit/latest/total.svg)
[![SE](https://img.shields.io/badge/SUBTITLE%20EDIT-join%20chat-blue.svg)](https://gitter.im/SubtitleEdit/subtitleedit "Subtitle Edit Gitter Chatroom")
[![download latest release](https://img.shields.io/badge/SUBTITLE%20EDIT-download-000F39.svg)](https://github.com/SubtitleEdit/subtitleedit/releases/latest)

Subtitle Edit is a free, open source editor for subtitle files. It lets you create, edit, synchronize, and quality‑check subtitles with powerful tools like waveform/spectrogram views, spell checking, OCR, and auto‑translation.

Helpful docs: https://www.nikse.dk/SubtitleEdit/Help

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate?hosted_button_id=4XEHVLANCQBCU)

## Key features

- Edit and preview subtitles with waveform and spectrogram
- Powerful synchronization tools (visual sync, point sync, shift, stretch)
- Quality checks and auto‑fix for common errors
- Spell check with dictionaries and custom word lists
- OCR to/from hard‑subbed video frames (Tesseract, PaddleOCR)
- Speech‑to‑text import via Whisper (where supported)
- Batch convert and export to custom text formats
- Translate using online services (when configured)
- Supports right‑to‑left languages and complex scripts
- Works with hundreds of subtitle formats (e.g., `srt`, `ass/ssa`, `vtt`, `microdvd`, `ttml`, `dost`, and many more)

## Download

- Get the latest Windows installer or portable zip from the Releases page: https://github.com/SubtitleEdit/subtitleedit/releases/latest
- Official website: https://www.nikse.dk/SubtitleEdit

## Build from source

Prerequisites:

- Windows 10/11
- Visual Studio 2022 (or JetBrains Rider) with .NET desktop development workload
- .NET Framework 4.8.1 Developer Pack

Steps:

1. Clone the repository
2. Open `SubtitleEdit.sln`
3. Set startup project to `src\ui`
4. Build and run (Debug | Any CPU)

Notes:

- Optional OCR and STT components (Tesseract, PaddleOCR, Whisper) are included/bundled where applicable. Some features may download additional data on first use.
- The library project lives in `src\libse` and is packed with `Readme.md` for NuGet.

## Using Subtitle Edit (quick start)

- Open a subtitle (`File > Open`) or create a new one
- Load a video to preview and see waveform/spectrogram
- Adjust timing with synchronization tools
- Run `Tools > Fix common errors` and spell check
- Save as your preferred format (`File > Save As`)

## Contributing

Issues, PRs, and feature requests are welcome:

- Issues: https://github.com/SubtitleEdit/subtitleedit/issues
- Discussions/Chat: https://gitter.im/SubtitleEdit/subtitleedit

When contributing code:

- Follow the existing C# style in the module you modify
- Keep changes focused and include tests when fixing a bug in `libse`
- For UI strings, provide English text; translations are handled via language files

## Translations

Subtitle Edit is localized into many languages. If you want to help improve a translation, see the language XML files in the repo and the guidance on the website: https://www.nikse.dk/SubtitleEdit/Help

## Privacy and license

- Privacy: see `Privacy.md`
- License: see `LICENSE.txt` (GPL)

## Acknowledgements

Subtitle Edit includes or integrates with third‑party components such as Tesseract OCR, PaddleOCR, and Whisper. Thanks to all contributors and the open source community!