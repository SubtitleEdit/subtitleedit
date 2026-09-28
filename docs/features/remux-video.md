# Remux Video

Repackage the video with new audio tracks and optional soft subtitles into an MP4, MKV or MOV file without re-encoding the video.

- **Menu:** Video → More → Remux video...
- **Shortcut:** None (menu only)

<!-- Screenshot: Remux video window with the video, audio and subtitle sections (remux-video.png, not yet taken) -->

Remuxing copies the streams into a new container, so it is quick and lossless. Use it to attach a dubbed or translated audio track (for example from [Text to Speech](text-to-speech.md)), to add subtitle tracks that can be switched on and off in the player, to embed CEA-608 closed captions, or to convert between MP4, MKV and MOV. To draw the subtitle into the picture instead, see [Burn-In Subtitles](burn-in.md).

The **More** submenu is shown while a video is loaded, and ffmpeg is required (you are offered to download it when it is missing).

## How to Use

1. Open **Video → More → Remux video...**
2. **Input video** is filled with the loaded video, and its own audio is placed first in the audio list (with several audio tracks you are asked which one to use)
3. **Add...** the audio files and, optionally, the subtitle files
4. Pick the **Output format** and check the **Output file** name
5. Click **Remux**; a progress bar shows how far ffmpeg has come
6. When done, a file-saved prompt appears and **Open containing folder** and **Play** buttons show up next to **Done**

**Cancel** while remuxing stops ffmpeg and deletes the partial output file. The window is not modal, so the main window stays usable while it is open; only one remux window can be open at a time.

## Input Video

Accepted formats: `.mp4`, `.mkv`, `.mov`, `.avi`, `.webm`, `.ts`. The first video stream is copied as-is.

## Audio Files

Accepted formats: `.mp3`, `.aac`, `.ac3`, `.wav`, `.mkv`, `.mka`, `.mp4`. At least one audio file is required.

- Each file becomes one audio track, in list order - use **Move up** / **Move down** (Ctrl+Up / Ctrl+Down) to change the order, **Remove** (Delete) or **Clear** to drop entries
- A file with several audio tracks asks which track to use when it is added; **Select audio track...** in the list's right-click menu changes it later
- The video's own audio is included only while it is in the list - remove it to replace the original sound entirely
- Each track gets a title (the file name, or the track name for multi-track files) and, when known, its language
- For an `.mp4` or `.mov` output, `.wav` audio is encoded to AAC (192 kb/s); everything else is copied

## Subtitle Files

Accepted formats: `.srt`, `.ass`, `.ssa`, `.vtt`, `.sub`, `.scc`. These become soft subtitle tracks (selectable in the player, not burned in), one per file, titled after the file name. In an MKV the file is stored unchanged; in an MP4 it is converted to the MP4 text subtitle format (mov_text), which loses styling.

A Scenarist `.scc` file is embedded as a CEA-608 closed caption track (QuickTime `c608`), which only the MOV container supports. Save the subtitle as *Scenarist Closed Captions* first, then add the `.scc` file here.

## Output Format and File

- **Output format** is `.mp4`, `.mkv` or `.mov`; the default follows the input video (MKV in, MKV out; MOV in, MOV out; anything else MP4)
- **MOV is required**, and switched to automatically with a message, when there is an `.scc` subtitle. MOV also holds several audio and subtitle tracks
- **MKV is required**, and switched to automatically with a message, when there is more than one audio file, more than one subtitle file (unless the output is MOV), or an `.ass`/`.ssa` subtitle (to keep its styles)
- **Output file** defaults to the video name with `_remuxed` appended, next to the video; a number is added if that file exists. It cannot be one of the input files

The window remembers its size and position.

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Ctrl+Up / Ctrl+Down | Move the selected audio or subtitle file up / down |
| Delete | Remove the selected file from its list |
| Escape | Close (not while remuxing) |
| F1 | Open help |
