# Sync Subtitle to Audio

Find the one offset that puts a whole subtitle in sync with the speech in the video. If the subtitle was also made at a different frame rate, that is found as well.

- **Menu:** Video → More → Sync subtitle to audio...

This tool is for a subtitle that is out of sync by the same amount everywhere, such as a subtitle for another release of the same cut. It compares when text is shown with when someone speaks, and moves every line by the offset that matches best. A subtitle that drifts or is out of sync by different amounts in different parts needs [Point Sync](point-sync.md) or [Improve Time Codes](improve-time-codes.md) instead.

## How to Use

1. Open the subtitle and the video, and let the waveform be generated
2. Open **Video → More → Sync subtitle to audio...**
3. Choose what to sync against (see below)
4. Click **Analyze**. A progress strip shows how far it has come
5. Check the **Result**, then click **Apply**

**Apply** moves all lines in one step, so a single **Undo** takes it back. **Cancel** closes the window without changing anything.

## Sync Against

- **Speech in the video's audio (from the waveform)**: finds where people speak in the audio. It is available only after the waveform has been generated
- **Another subtitle file that is already in sync**: uses the timing of a subtitle that is known to be right, for example one in another language. Click the **...** button to pick the file

## Options

- **Max offset (seconds)**: the largest shift that is tried, in either direction. The default is 120 seconds. Raise it for a subtitle that is far out, such as one that includes or skips an intro
- **Also try frame rate conversions (23.976, 24, 25, 29.97, 30)**: also tries every conversion between these rates, for a subtitle made for a PAL or NTSC version of the video

Both options are remembered for the next time.

## Result

- **Offset**: how far the lines will move
- **Frame rate**: the conversion that matched best, or "unchanged"
- **Confidence**: how well the subtitle matched the audio or the reference subtitle

When the confidence is low, the subtitle may belong to another cut of the video, or the audio may be mostly music. Check a few lines before you save.
