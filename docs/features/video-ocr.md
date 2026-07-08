# OCR Burned-in Subtitle (Video OCR)

Extract hardcoded (burned-in) subtitles from a video into editable text lines using OCR.

- **Menu:** Video → OCR burned-in subtitle...

## How to Use

1. Open **Video → OCR burned-in subtitle...** (a video file is required)
2. Use the preview slider to find a frame that shows a subtitle
3. Adjust the scan area rectangle so it covers where the subtitles appear (default: bottom third) — drag to move/resize, or use the preset buttons
4. Pick an OCR engine and language
5. Click **Start OCR** — lines appear in the list as they are recognized
6. Click **OK** to load the result into the main window

## OCR Engines

- **Paddle OCR** — local, fast and accurate; downloaded automatically (Windows/Linux). Recommended.
- **Paddle OCR Python** — local, via a Python `paddleocr` installation (also works on macOS)
- **Ollama vision** — local vision model via [Ollama](https://ollama.com), e.g. `glm-ocr`
- **GLM API** — GLM vision model via the Z.ai / bigmodel.cn cloud API (requires an API key)

## How It Works

Frames are sampled from the scan area at a few frames per second with ffmpeg. Consecutive
near-identical frames are collapsed so each on-screen subtitle is OCR'ed only once, then
consecutive OCR results with near-identical text are merged into one line with correct start/end
times (the text variant shown the longest wins).

## Settings

- **Frames per second** — how many frames per second to sample (higher = more precise timing, slower)
- **Text brightness minimum** — pixels darker than this are ignored when comparing frames, so the
  comparison follows the (bright) subtitle text instead of the moving video behind it; frames with
  no bright pixels are skipped entirely. Set to 0 to disable (e.g. for dark subtitle text).
- **Merge lines with similarity (%)** — how similar the text of two consecutive OCR results must be to merge into one line
- **Max gap between lines (ms)** — maximum time gap allowed when merging
- **Minimum duration (ms)** — lines shorter than this are dropped (removes OCR blips/false positives)
- **Add ASSA position tag** — prepend an alignment tag (e.g. `{\an8}` for a top scan area) based on
  the scan area position, for Advanced Sub Station Alpha output

## Tips

- Scan only the region where subtitles actually appear — a smaller area is faster and has fewer false positives
- For subtitles at the top of the frame (e.g. sign translations), move the scan area up and enable the ASSA position tag
- If lines are duplicated with small OCR differences, raise **Merge lines with similarity** tolerance by lowering the percentage
- If short random text blips appear, raise **Minimum duration**
