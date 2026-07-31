# Edit Image-based Subtitle

Edit image-based subtitles — Blu-ray SUP, VobSub, DVB, BDN XML — directly, without OCR: adjust timing and position, resize, recolor, set forced flags, and export to a range of image-based formats.

- **Menu:** File → Import → Image-based subtitle for edit...
- **Also:** The **Edit/export...** button in the [OCR window](ocr.md) opens the loaded subtitle here
- **Shortcut:** Configurable (no default)

<!-- Screenshot: Edit image-based subtitle window (binary-edit.png, not yet taken) -->

## Supported Files

- Blu-ray SUP (`.sup`)
- VobSub (`.sub` + `.idx`)
- Transport stream (`.ts`) with DVB subtitles
- Matroska (`.mkv`/`.mks`) with PGS, VobSub, or DVB tracks
- BDN XML (`.xml`)

## Window Layout

The subtitle list shows the forced flag, number, start time, duration, and the image itself. Below it you can edit the selected line's show time, duration, X/Y position, and the screen size. Double-click a line to seek the video to it; Delete removes selected lines.

The right panel toggles between:

- **Video** — Video preview with the subtitle image overlaid at its real position; drag the image with the mouse to move it. A matching video file next to the subtitle is opened automatically
- **Position map** — An overview of where all subtitles sit on screen, with clickable counts for lines in the picture, bottom bar, and top bar. Pick a letterbox aspect ratio (or a custom bar height) and a title-safe margin to check that positioned subtitles stay inside the bars

## Tools

From the Tools menu (all lines) or the right-click menu (selected lines):

- **Alignment** / **Center horizontally** / **Top align** / **Bottom align** — Reposition using the margins from the window's Options → Settings
- **Resize images...** — Scale the bitmaps by a percentage
- **Crop images** — Trim transparent borders
- **Adjust brightness...** / **Adjust alpha (transparency)...** / **Adjust color...** — Image corrections with live preview
- **Adjust durations...** / **Apply duration limits...** — Same duration tools as for text subtitles
- **Set text** — Replace a line's image with newly rendered text (font, colors, outline, shadow, box)
- **Append subtitle...** — Append another image-based file, keeping its time codes or offsetting them
- **Sort by start time**, **Toggle forced**, **Select forced/non-forced lines**, **Import time codes...**

The Synchronization menu offers **Adjust all times**, **Change frame rate**, and **Change speed**.

## Export

There is no in-place save — use File → Export:

- **Blu-ray (sup)**, **VobSub (sub/idx)**
- **BDN/xml**, **DOST/png**, **Final Cut Pro + image**
- **IMSC 1.1 image profile**, **WebVTT png**
- **Images with HTML index**, **Images with time code**

The **Export** button under the list saves the selected line's image as a PNG. Closing with unexported changes asks for confirmation.
