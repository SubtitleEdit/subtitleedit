# WebVTT (Web Video Text Tracks) Format Reference

WebVTT (file extension `.vtt`) is a modern subtitle format designed for HTML5 video, offering powerful styling and positioning capabilities.

## Overview

WebVTT (Web Video Text Tracks) was developed by the W3C (World Wide Web Consortium) as the official subtitle format for HTML5 video. It's based on SubRip (SRT) but with significant enhancements for web-based video playback.

**Key Features:**
- Official W3C standard with [complete specification](https://www.w3.org/TR/webvtt1/)
- Native support in all modern web browsers
- CSS-based styling system
- Precise positioning and alignment
- Cue settings for layout control
- Support for metadata tracks
- Chapter markers and descriptions
- Ruby text (for East Asian languages)
- Voice tagging for speaker identification

**Pros:**
- ✅ Official web standard with complete specification
- ✅ Native browser support (no plugins needed)
- ✅ Powerful CSS-based styling
- ✅ Precise positioning control
- ✅ Support for metadata and chapters
- ✅ Human-readable text format

**Cons:**
- ❌ Less supported in desktop video players than SRT
- ❌ More complex than SubRip
- ❌ CSS support varies by browser
- ❌ Requires understanding of cue settings

## File Structure

A WebVTT file starts with a mandatory signature line, followed by optional header and cue blocks.

### Basic Structure

```
WEBVTT

00:00:01.000 --> 00:00:03.000
Subtitle text here

00:00:03.500 --> 00:00:05.500
Another subtitle
```

### Complete Structure

```
WEBVTT [optional description]

NOTE
Optional comment block

STYLE
::cue {
  background-color: rgba(0,0,0,0.8);
}

REGION
id:region1
width:40%
lines:3
regionanchor:0%,100%
viewportanchor:10%,90%

[cue identifier]
00:00:01.000 --> 00:00:03.000 [cue settings]
Subtitle text
```

## Timecode Format

```
hours:minutes:seconds.milliseconds --> hours:minutes:seconds.milliseconds
```

**Important notes:**
- Hours can be omitted if less than 1 hour: `00:30.000` is valid
- Period (`.`) is used as decimal separator (not comma like SRT)
- Milliseconds use 3 digits
- Arrow separator is ` --> ` (space-arrow-space)
- Negative timestamps are allowed (for pre-roll)

**Examples:**
```
00:00:01.000 --> 00:00:03.500
01:15.250 --> 01:18.750
1:30:45.100 --> 1:30:50.900
-00:00:02.000 --> 00:00:00.000
```

## Basic Example

```vtt
WEBVTT

00:00:01.000 --> 00:00:03.000
Hello, World!

00:00:03.500 --> 00:00:05.500
This is WebVTT format.

00:00:06.000 --> 00:00:08.000
It supports multiple
lines of text.
```

## Mandatory Signature

Every WebVTT file **must** start with the string `WEBVTT` on the first line.

**Valid signatures:**
```vtt
WEBVTT
```

```vtt
WEBVTT - Optional description here
```

```vtt
WEBVTT FILE This file contains subtitles
```

**Invalid:**
```
webvtt          (lowercase not allowed)
 WEBVTT         (space before signature)
WEBVTT subtitle (hyphen required before description)
```

## Text Formatting

WebVTT supports HTML-like formatting tags and CSS classes.

### Basic Tags

#### Bold
```vtt
<b>Bold text</b>
```

#### Italic
```vtt
<i>Italic text</i>
```

#### Underline
```vtt
<u>Underlined text</u>
```

### Voice Tags

Identify speakers using voice tags:

```vtt
<v John>Hello, I'm John.

<v.loud John>I'M SHOUTING!

<v Mary>Hi John, nice to meet you.
```

### Class Tags

Apply CSS classes for styling:

```vtt
<c>Text with default class</c>
<c.yellow>Yellow text</c>
<c.yellow.bg_blue>Yellow text on blue background</c>
```

**Predefined color classes:**
- `white`, `lime`, `cyan`, `red`, `yellow`
- `magenta`, `blue`, `black`, `green`

### Ruby Text

For East Asian languages (Japanese, Chinese):

```vtt
<ruby>漢<rt>かん</rt>字<rt>じ</rt></ruby>
```

### Language Tags

```vtt
<lang en>Hello</lang>
<lang ja>こんにちは</lang>
```

### Combining Tags

```vtt
<v John><i>thinking...</i></v>
<c.yellow><b>Important!</b></c>
```

## Cue Settings

Cue settings control the positioning and layout of subtitles.

### Syntax

```
00:00:01.000 --> 00:00:03.000 position:50% align:center
Text here
```

### Available Settings

| Setting | Description | Values | Default |
|---------|-------------|--------|---------|
| `position` | Horizontal position | 0-100% or `auto` | 50% |
| `line` | Vertical position | number, % or `auto` | `auto` |
| `size` | Width of cue box | 0-100% | 100% |
| `align` | Text alignment | `start`, `center`, `end`, `left`, `right` | `center` |
| `vertical` | Vertical text | `rl` (right-to-left), `lr` (left-to-right) | horizontal |
| `region` | Region identifier | string | none |

### Position Examples

```vtt
WEBVTT

00:00:01.000 --> 00:00:03.000 position:10% align:start
Left side text

00:00:03.500 --> 00:00:05.500 position:50% align:center
Centered text (default)

00:00:06.000 --> 00:00:08.000 position:90% align:end
Right side text

00:00:08.500 --> 00:00:10.500 line:0% position:50%
Top of screen

00:00:11.000 --> 00:00:13.000 line:85% position:50%
Near bottom

00:00:13.500 --> 00:00:15.500 size:50% position:25%
Narrow subtitle box
```

## CSS Styling

WebVTT supports embedded CSS for custom styling.

### Style Block

```vtt
WEBVTT

STYLE
::cue {
  background-color: rgba(0, 0, 0, 0.8);
  color: white;
  font-size: 1.5em;
  font-family: Arial, sans-serif;
}

::cue(b) {
  color: yellow;
}

::cue(.yellow) {
  color: yellow;
}

::cue(.red) {
  color: red;
}

00:00:01.000 --> 00:00:03.000
Normal white text

00:00:03.500 --> 00:00:05.500
<b>Yellow bold text</b>

00:00:06.000 --> 00:00:08.000
<c.red>Red text</c>
```

### Supported CSS Properties

- `color` — Text color
- `background-color` — Background color
- `opacity` — Transparency (0.0 to 1.0)
- `font-family` — Font family
- `font-size` — Font size
- `font-weight` — Bold (normal, bold, 100-900)
- `font-style` — Italic (normal, italic)
- `text-decoration` — Underline (none, underline)
- `text-shadow` — Text shadow effect
- `white-space` — Whitespace handling

**Example:**
```css
::cue {
  color: #ffffff;
  background-color: rgba(0, 0, 0, 0.9);
  font-family: 'Arial', sans-serif;
  font-size: 20px;
  text-shadow: 2px 2px 4px #000000;
}
```

## Regions

Regions define areas of the video where subtitles can appear.

### Region Definition

```vtt
WEBVTT

REGION
id:bill
width:40%
lines:3
regionanchor:0%,100%
viewportanchor:10%,90%
scroll:up

REGION
id:fred
width:40%
lines:3
regionanchor:100%,100%
viewportanchor:90%,90%
scroll:up

00:00:01.000 --> 00:00:03.000 region:bill
<v Bill>I'm on the left

00:00:01.000 --> 00:00:03.000 region:fred
<v Fred>I'm on the right
```

### Region Properties

| Property | Description | Values |
|----------|-------------|--------|
| `id` | Unique identifier | string |
| `width` | Region width | percentage |
| `lines` | Number of lines | integer |
| `regionanchor` | Anchor point in region | x%,y% |
| `viewportanchor` | Anchor point in viewport | x%,y% |
| `scroll` | Scroll behavior | `up` or none |

## Cue Identifiers

Cues can have optional identifiers for scripting and navigation.

```vtt
WEBVTT

intro
00:00:01.000 --> 00:00:03.000
Welcome to the video

dialog-1
00:00:03.500 --> 00:00:05.500
<v John>Hello there!

dialog-2
00:00:05.500 --> 00:00:07.500
<v Mary>Hi John!
```

## Comments

Use `NOTE` blocks for comments:

```vtt
WEBVTT

NOTE
This is a comment block.
It can span multiple lines.

NOTE Another comment on one line

00:00:01.000 --> 00:00:03.000
Subtitle text
```

## Complete Example

```vtt
WEBVTT - Example subtitle file

NOTE
Created with Subtitle Edit
For demonstration purposes

STYLE
::cue {
  background-color: rgba(0, 0, 0, 0.8);
  color: white;
  font-family: Arial, sans-serif;
}

::cue(.yellow) {
  color: yellow;
}

::cue(v[voice="John"]) {
  color: cyan;
}

REGION
id:left
width:40%
regionanchor:0%,100%
viewportanchor:10%,90%

REGION
id:right
width:40%
regionanchor:100%,100%
viewportanchor:90%,90%

intro
00:00:01.000 --> 00:00:03.000 position:50% align:center
<c.yellow><b>Welcome to WebVTT</b></c>

narrator
00:00:03.500 --> 00:00:06.500
<i>Narrator speaking...</i>

dialog-1
00:00:07.000 --> 00:00:09.000 region:left
<v John>Hello, I'm on the left side.

dialog-2
00:00:09.000 --> 00:00:11.000 region:right
<v Mary>And I'm on the right!

caption-1
00:00:12.000 --> 00:00:15.000 line:10% position:50%
Top-positioned subtitle

caption-2
00:00:16.000 --> 00:00:18.000 size:50% position:25% align:start
<b>Bold text</b> with <i>italic</i> and <u>underline</u>
```

## Character Encoding

WebVTT files must be encoded as:
- **UTF-8** (required by specification)

**Important:** UTF-8 is the only valid encoding for WebVTT files according to the W3C specification.

## Timestamps and Overlapping

- Cues can overlap in time (multiple subtitles on screen simultaneously)
- Cues don't need to be in chronological order (but recommended)
- Start time can equal end time for zero-duration cues
- Negative timestamps are allowed for pre-roll content

**Example:**
```vtt
WEBVTT

00:00:01.000 --> 00:00:05.000 line:85%
Bottom subtitle (stays for 4 seconds)

00:00:02.000 --> 00:00:04.000 line:15%
Top subtitle (appears while bottom is still showing)
```

## Common Issues and Solutions

### Issue: File not recognized
**Problem:** Missing or incorrect WEBVTT signature
```
Wrong: webvtt (lowercase)
Wrong:  WEBVTT (space before)
Right: WEBVTT
Right: WEBVTT - My subtitles
```

### Issue: Styling not working
**Problem:** Browser doesn't support custom CSS or cue settings
**Solution:** Test in different browsers; fallback to basic tags

### Issue: Special characters display incorrectly
**Problem:** File not saved as UTF-8
**Solution:** Always save WebVTT files as UTF-8 (required by spec)

### Issue: Multiple subtitles not showing simultaneously
**Problem:** Player or browser limitation
**Solution:** Use regions or test in different player

### Issue: Position settings ignored
**Problem:** Using comma instead of period in timecodes
```
Wrong: 00:00:01,000 --> 00:00:03,000
Right: 00:00:01.000 --> 00:00:03.000
```

## Best Practices

1. **Always start with WEBVTT:** The signature is mandatory
2. **Use UTF-8 encoding:** Required by the specification
3. **Add descriptions:** Use the signature line for file info
4. **Use cue identifiers:** Makes navigation and scripting easier
5. **Test in browsers:** WebVTT is designed for HTML5 video
6. **Use voice tags:** Clarify who is speaking
7. **Use regions wisely:** For complex multi-speaker scenes
8. **Comment your code:** Use NOTE blocks for documentation
9. **Follow timing guidelines:** Same as SRT (see SubRip documentation)
10. **Validate your files:** Use W3C validators

## Metadata Tracks

WebVTT can be used for metadata, not just subtitles:

```vtt
WEBVTT

00:00:00.000 --> 00:00:10.000
{"title": "Introduction", "slide": 1}

00:00:10.000 --> 00:00:30.000
{"title": "Main Content", "slide": 2}
```

## Chapter Tracks

Define video chapters:

```vtt
WEBVTT

chapter-1
00:00:00.000 --> 00:05:00.000
Introduction

chapter-2
00:05:00.000 --> 00:15:00.000
Main Content

chapter-3
00:15:00.000 --> 00:20:00.000
Conclusion
```

## Converting to WebVTT

Subtitle Edit can convert many formats to WebVTT:

1. Open your subtitle file
2. Go to **File** → **Save as...**
3. Select **WebVTT (*.vtt)** from format dropdown
4. Click **Save**

## Browser Support

WebVTT is natively supported in:
- ✅ Chrome/Chromium (full support)
- ✅ Firefox (full support)
- ✅ Safari (full support)
- ✅ Edge (full support)
- ✅ Opera (full support)

**Note:** The level of CSS styling support may vary between browsers.

## Tools and Resources

### Recommended Tools
- **Subtitle Edit** — Full-featured editor with WebVTT support
- **Aegisub** — Can export to WebVTT
- **Browser DevTools** — For testing HTML5 video with WebVTT

### Validation
- [W3C WebVTT Validator](https://w3c.github.io/webvtt.js/parser.html)
- Use browser console to check for parsing errors

### Documentation
- [W3C WebVTT Specification](https://www.w3.org/TR/webvtt1/) — Official spec
- [MDN WebVTT Guide](https://developer.mozilla.org/en-US/docs/Web/API/WebVTT_API) — Mozilla documentation
- [HTML5 Video](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/video) — Using WebVTT with video

## Technical Specifications

| Property | Value |
|----------|-------|
| **File Extension** | `.vtt`, `.webvtt` |
| **MIME Type** | `text/vtt` |
| **Character Encoding** | UTF-8 (required) |
| **Line Endings** | Any (CRLF, LF, CR) |
| **Timecode Format** | `HH:MM:SS.mmm` or `MM:SS.mmm` |
| **Timecode Precision** | Milliseconds (3 digits) |
| **Specification** | [W3C WebVTT](https://www.w3.org/TR/webvtt1/) |

## HTML5 Integration

### Basic Usage

```html
<video controls>
  <source src="video.mp4" type="video/mp4">
  <track label="English" kind="subtitles" srclang="en" src="subtitles.vtt" default>
  <track label="Spanish" kind="subtitles" srclang="es" src="subtitles_es.vtt">
  <track label="Chapters" kind="chapters" srclang="en" src="chapters.vtt">
</video>
```

### Track Kinds

- `subtitles` — Translations (default)
- `captions` — Transcription and sound effects
- `descriptions` — Text descriptions of visuals
- `chapters` — Chapter titles for navigation
- `metadata` — Metadata for scripts

## See Also

- [SubRip Format](subrip.md) — Simpler alternative format
- [ASSA Format](assa.md) — More powerful desktop subtitle format
- [ASSA Override Tags](assa-override-tags.md) — ASS-style tags reference
- [Supported Formats](supported-formats.md) — All formats supported by Subtitle Edit
- [Command Line](command-line.md) — Batch conversion with Subtitle Edit

## Version History

- **2010** — WebVTT development begins at WHATWG
- **2019** — W3C WebVTT 1.0 becomes official recommendation
- **Present** — Standard format for HTML5 video subtitles

**Note:** WebVTT has a complete official specification, unlike SubRip which is defined by common practice.
