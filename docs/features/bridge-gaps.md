# Bridge Gaps

Extend subtitle durations to fill gaps between consecutive subtitles.

- **Menu:** Tools → Bridge gaps...

<!-- Screenshot: Bridge gaps window -->
![Bridge Gaps](../screenshots/bridge-gaps.png)

## Options

- **Bridge gaps smaller than** — Only bridge gaps smaller than this value
- **Min. gap** — Keep at least this much gap between the bridged subtitles
- **Percent for previous** — How much of the gap is given to the previous subtitle (the rest goes to the next subtitle)

Both values are entered in milliseconds, or in frames when the global *Use frame mode (hh.mm.ss.ff)* setting is enabled - the labels show which, and the gap column in the preview uses the same unit. Frames are counted at the current frame rate. The frame and millisecond values are remembered separately, so switching the time format never reads one as the other. The *Bridge gaps* step in Batch convert follows the same setting.

In millisecond mode, the **...** button next to each value opens a small calculator that turns a number of frames at a chosen frame rate into milliseconds - handy when a style guide gives the gaps in frames.

The preview updates live and the status text shows the number of bridged gaps.
