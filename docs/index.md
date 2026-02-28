# Subtitle Edit - Documentation

Subtitle Edit is a free, open-source editor for video subtitles. This is the documentation for the Avalonia-based cross-platform version (Subtitle Edit 5).

## Getting Started

- [Overview](overview.md) — What is Subtitle Edit and how to get started
- [Main Window](features/main-window.md) — Main window layout, areas, and interaction guide
- [FAQ](faq.md) — Frequently Asked Questions

## Feature Areas

### File Operations
- [File Menu](features/file.md) — New, Open, Save, Import, Export

### Editing
- [Edit Menu](features/edit.md) — Find, Replace, Multiple Replace, Modify Selection, History
- [Subtitle Grid](features/subtitle-grid.md) — Working with the subtitle list/grid
- [Text Editor](features/text-editor.md) — Editing subtitle text

### Tools
- [Fix Common Errors](features/fix-common-errors.md) — Automatic error detection and fixing
- [Batch Convert](features/batch-convert.md) — Convert multiple subtitle files
- [Change Casing](features/change-casing.md) — Fix casing issues
- [Change Formatting](features/change-formatting.md) — Modify subtitle formatting
- [Adjust Duration](features/adjust-duration.md) — Adjust subtitle display durations
- [Apply Duration Limits](features/apply-duration-limits.md) — Enforce min/max duration rules
- [Bridge Gaps](features/bridge-gaps.md) — Bridge gaps between subtitles
- [Apply Minimum Gap](features/apply-min-gap.md) — Set minimum gap between subtitles
- [Merge Short Lines](features/merge-short-lines.md) — Merge short subtitle lines
- [Merge Lines with Same Text](features/merge-same-text.md) — Merge duplicate text lines
- [Merge Lines with Same Time Codes](features/merge-same-timecodes.md) — Merge lines with identical timings
- [Split/Break Long Lines](features/split-break-long-lines.md) — Break overly long lines
- [Split Subtitle](features/split-subtitle.md) — Split one subtitle file into multiple files
- [Join Subtitles](features/join-subtitles.md) — Join multiple subtitle files
- [Sort By](features/sort-by.md) — Sort subtitles by various criteria
- [Remove Text for Hearing Impaired](features/remove-text-hi.md) — Remove HI annotations

### Synchronization
- [Adjust All Times](features/adjust-all-times.md) — Shift all timings
- [Visual Sync](features/visual-sync.md) — Synchronize visually with video
- [Point Sync](features/point-sync.md) — Sync using reference points
- [Point Sync Via Other Subtitle](features/point-sync-via-other.md) — Sync using another subtitle
- [Change Frame Rate](features/change-frame-rate.md) — Convert between frame rates
- [Change Speed](features/change-speed.md) — Adjust playback speed

### Video
- [Video Player](features/video-player.md) — Video playback controls
- [Audio Visualizer / Waveform](features/audio-visualizer.md) — Waveform display and editing
- [Speech to Text (Whisper)](features/speech-to-text.md) — Automatic speech recognition
- [Text to Speech](features/text-to-speech.md) — Generate speech from subtitles
- [Burn-In Subtitles](features/burn-in.md) — Hardcode subtitles into video
- [Transparent Subtitles](features/transparent-subtitles.md) — Generate transparent subtitle video
- [Shot Changes](features/shot-changes.md) — Detect and manage shot changes
- [Blank Video](features/blank-video.md) — Generate a blank video
- [Re-encode Video](features/re-encode-video.md) — Re-encode video files
- [Cut Video](features/cut-video.md) — Cut video segments

### Translation
- [Auto Translate](features/auto-translate.md) — Automatic subtitle translation
- [Copy/Paste Translate](features/copy-paste-translate.md) — Translate via copy/paste workflow

### Spell Check
- [Spell Check](features/spell-check.md) — Spell checking subtitles
- [Find Double Words](features/find-double-words.md) — Find repeated words

### OCR (Optical Character Recognition)
- [OCR](features/ocr.md) — Convert image-based subtitles to text (nOCR, Binary OCR, Tesseract)

### ASSA (Advanced SubStation Alpha)
- [ASSA Styles](features/assa-styles.md) — Manage ASS/SSA styles
- [ASSA Properties](features/assa-properties.md) — Edit script properties
- [ASSA Attachments](features/assa-attachments.md) — Manage font/image attachments
- [ASSA Draw](features/assa-draw.md) — Vector drawing tool
- [ASSA Set Position](features/assa-set-position.md) — Position subtitles on screen
- [ASSA Set Background](features/assa-set-background.md) — Add background boxes
- [ASSA Progress Bar](features/assa-progress-bar.md) — Generate progress bars
- [ASSA Resolution Resampler](features/assa-resolution-resampler.md) — Resample for different resolutions
- [ASSA Image Color Picker](features/assa-image-color-picker.md) — Pick colors from video
- [ASSA Apply Custom Override Tags](features/assa-override-tags.md) — Apply custom tags

### Options
- [Settings](features/settings.md) — Application settings
- [Shortcuts](features/shortcuts.md) — Keyboard shortcuts
- [Word Lists](features/word-lists.md) — Custom word lists and dictionaries

### Other
- [Compare](features/compare.md) — Compare subtitle files
- [Statistics](features/statistics.md) — Subtitle statistics

## Reference

### Format Documentation
- [Supported Formats](reference/supported-formats.md) — List of all supported subtitle formats
- [SubRip (SRT) Format](reference/subrip.md) — Complete SubRip format reference and guide
- [WebVTT Format](reference/webvtt.md) — Complete WebVTT format reference (HTML5 standard)
- [Advanced Sub Station Alpha (ASSA) Format](reference/assa.md) — Complete ASSA format reference and guide
- [ASSA Override Tags Reference](reference/assa-override-tags.md) — Complete ASSA/SSA override tags reference

### Application Reference
- [Keyboard Shortcuts Reference](reference/keyboard-shortcuts.md) — Complete list of keyboard shortcuts
- [Mouse Controls Reference](reference/mouse-controls.md) — Mouse interactions
- [Command Line (seconv)](reference/command-line.md) — Command-line converter
- [Third-Party Components](third-party-components.md) — FFmpeg, MPV, Tesseract, Whisper setup guide
