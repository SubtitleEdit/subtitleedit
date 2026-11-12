# Sync Audio Track

Move one audio track of a video earlier or later, for example a dubbed track that is out of sync with the picture, and save a copy of the video with the track moved.

- **Menu:** Video → Sync audio track...

Only the chosen audio track is moved. The video and the other tracks are copied as they are, and nothing is re-encoded, so saving is quick and lossless. ffmpeg is required (you are offered to download it when it is missing). With no video open, you are asked to pick one.

## How to Use

1. Open **Video → Sync audio track...**
2. When the video has more than one audio track, pick the **Audio track** to move
3. Set the **Offset (ms)**. Type a value, or use the buttons to move it by -1000, -100, -10, +10, +100 or +1000 ms. Positive values play the audio later, negative values earlier
4. Watch the video in the window. It plays with the offset applied, so you can hear when the speech matches the lips
5. Click **Save video...** and choose where to save the copy. The suggested name ends in `_synced`

**Cancel** while saving stops ffmpeg. Otherwise it closes the window.

## Preview

The live preview of the offset needs the mpv video player (**Options → Settings → Video**). With another player the preview plays without the offset, but the saved video gets the offset all the same.

## See Also

- [Sync Subtitle to Audio](sync-to-audio.md) moves the subtitle to match the audio instead
- [Remux Video](remux-video.md) adds or replaces audio tracks
