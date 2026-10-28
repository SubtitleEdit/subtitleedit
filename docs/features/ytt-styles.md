# YouTube Timed Text Styles

YouTube timed text (`.ytt`, also called srv3) is the format YouTube's player reads natively. Besides text and timing it has **pens** (text styles), **window positions** and **window styles**, plus per-word timing for karaoke. Subtitle Edit reads and writes all of them, previews them on the video like YouTube draws them, and has a style manager for them.

**Toolbar:** the styles button (shown when the current format is YouTube timed text)
**Right-click a line:** `YouTube` → line pen, position, window style
**Right-click in the text box:** `YouTube` → set pen for selection, karaoke start, remove pen tags

## How the Text Looks

A line has one pen, position and window style of its own, and parts of it can use another pen:

```
Plain text <s p="3">in pen 3</s> and <s t="1500">appears 1.5 s into the line</s>
```

- `<s p="N">` — this part uses pen *N*.
- `<s t="ms">` — karaoke: this part appears *ms* milliseconds after the line starts.
- `<i>`, `<b>`, `<u>` and `<font color>` also work; they are turned into pens when the file is saved.
- `{\an8}` and the other ASSA alignment tags become a window position when saved.

The text box colors the pen number in the pen's text color, and the grid's "show formatting" mode shows pens as they look (bold, italic, color).

## Pens

| Setting | Attribute | Notes |
|---|---|---|
| Bold, italic, underline | `b`, `i`, `u` | |
| Text color + opacity | `fc`, `fo` | opacity 0–255 |
| Background + opacity | `bc`, `bo` | the box behind the text |
| Edge | `et`, `ec` | hard shadow, bevel, glow/outline, soft shadow; without a color the edge is dark gray and follows the text opacity |
| Font style | `fs` | Courier New, Times New Roman, Lucida Console, Roboto, Comic Sans, Monotype Corsiva, small caps |
| Size | `sz` | not a real percentage: YouTube draws `100 + (sz − 100) / 4` %, so 200 is 125% and 0 (the smallest) is 75%. The editor shows the real size. |
| Offset | `of` | subscript, normal, superscript |

Each color has its own on/off switch. An attribute that is off is left out of the file, and YouTube then uses the viewer's own caption settings for it.

## Positions

A position places a line: the **anchor point** (which corner or edge of the text box) is put at the **horizontal** and **vertical** percentage of the video. YouTube pulls every position in towards the center (`value × 0.96 + 2` %), so 0% is really 2% and 100% is 98% — the editor shows where the line really ends up, and the preview draws that 2–98% area as a dashed frame.

## Window Styles

- **Justify** — left, right or center for lines of different length.
- **Text direction** — left to right, right to left, vertical upright or vertical rotated, and the column order for vertical text.
- **Window box opacity** — YouTube can draw a second box behind the whole window. It is hidden by default, but viewers can turn it on by accident, so set it to 0 to keep it hidden.

## Saving for Upload

YouTube's upload and its apps have a few quirks. Subtitle Edit works around them when it saves (the same workarounds as YTSubConverter):

- An opacity of 255 is written as 254, and text color `#FFFFFF` as `#FEFEFE` — otherwise the upload drops them and the viewer's settings take over.
- A line at 0 ms is written at 1 ms — the Android app ignores the position of lines at 0 ms.
- A zero-width space is added after the first `<s>` of a line that has no other text — otherwise the upload removes that part's pen.
- Placeholder entries with id 0 are added, and ids are written in increasing order.

These are undone when the file is read back, so they never show up in the editor.

## Converting

Converting to another format turns pens into `<i>`, `<b>`, `<u>` and `<font color>` tags and positions into `{\an}` tags. Converting from another format turns those tags into pens and positions when the file is saved.
