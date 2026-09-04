# Bridge Gaps

Extend subtitle durations to fill gaps between consecutive subtitles.

- **Menu:** Tools → Bridge gaps...

<!-- Screenshot: Bridge gaps window -->
![Bridge Gaps](../screenshots/bridge-gaps.png)

## Options

- **Bridge gaps smaller than** — Only bridge gaps smaller than this value
- **Min. gap** — Keep at least this much gap between the bridged subtitles
- **Percent for previous** — How much of the gap is given to the previous subtitle (the rest goes to the next subtitle)

Both values are always entered in milliseconds; only the gap column in the preview switches to frames when the global *Use frame mode (hh.mm.ss.ff)* setting is enabled. The preview updates live and the status text shows the number of bridged gaps.
