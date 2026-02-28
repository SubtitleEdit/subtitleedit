# SubRip (SRT) Format Reference

SubRip (file extension `.srt`) is one of the most widely used subtitle formats, known for its simplicity and broad compatibility.

## Overview

SubRip was created by French programmer [Brain](https://web.archive.org/web/20031026223830/http://membres.lycos.fr/subrip/) in 2000 for the [SubRip](https://www.videohelp.com/software/Subrip) program (written in Borland Delphi). Despite being over two decades old, it remains the most popular subtitle format today and is the default format in Subtitle Edit.

**Key Features:**
- Simple, human-readable plain text format
- Can be edited in any text editor
- Widely supported by video players, editors, and converters
- Can be [embedded into Matroska (MKV) files](https://www.matroska.org/technical/subtitles.html)
- Supports basic formatting (italic, bold, underline, colors)
- Supports some ASS-style tags for advanced positioning

**Pros:**
- ✅ Very simple format, easy to learn and use
- ✅ Can be edited in any text editor
- ✅ Excellent support in players/converters
- ✅ Small file size
- ✅ Human-readable

**Cons:**
- ❌ No official specification
- ❌ Limited styling options compared to ASSA
- ❌ Inconsistent tag support across players
- ❌ No built-in font/size control in basic format

## File Structure

A SubRip file consists of sequential subtitle blocks, each containing:

1. **Sequence number** — A unique number for each subtitle (starting from 1)
2. **Timecode** — Start and end time separated by ` --> `
3. **Text** — One or more lines of subtitle text
4. **Blank line** — Separator between subtitle blocks

### Timecode Format

```
hours:minutes:seconds,milliseconds --> hours:minutes:seconds,milliseconds
```

**Important notes:**
- Hours, minutes, and seconds use 2 digits (zero-padded)
- Milliseconds use 3 digits
- Comma (`,`) is used as decimal separator (not period)
- Arrow separator is ` --> ` (space-arrow-space)

**Examples:**
```
00:00:01,000 --> 00:00:03,500
00:01:15,250 --> 00:01:18,750
01:30:45,100 --> 01:30:50,900
```

## Basic Example

```srt
1
00:00:01,000 --> 00:00:03,000
Hello, World!

2
00:00:03,080 --> 00:00:05,080
This is a simple subtitle.

3
00:00:05,160 --> 00:00:07,160
It supports multiple
lines of text.
```

## Text Formatting

SubRip originally supported only plain text, but modern players support HTML-like formatting tags.

### Italic
```srt
<i>This text is italic</i>
```

### Bold
```srt
<b>This text is bold</b>
```

### Underline
```srt
<u>This text is underlined</u>
```

### Font Color
```srt
<font color="#ff0000">Red text</font>
<font color="#00ff00">Green text</font>
<font color="#0000ff">Blue text</font>
```

**Note:** Colors are specified in hexadecimal RGB format (not BGR like ASS).

### Font Face and Size
Some players support:
```srt
<font face="Arial">Arial font</font>
<font size="24">Large text</font>
<font face="Courier" size="18" color="#ff0000">Combined attributes</font>
```

**Note:** Font face and size support varies widely between players. Test with your target player.

### Combining Formatting
```srt
<b><i>Bold and italic</i></b>
<font color="#ff0000"><b>Bold red text</b></font>
```

## Advanced Formatting

### ASSA-Style Override Tags

Many modern players support ASSA-style override tags in SubRip files, enclosed in curly braces `{}`:

#### Alignment
```srt
1
00:00:01,000 --> 00:00:03,000
{\an8}Top-aligned text

2
00:00:03,000 --> 00:00:05,000
{\an2}Bottom-centered (default)

3
00:00:05,000 --> 00:00:07,000
{\an7}Top-left corner
```

**Alignment values (numpad-style):**
```
7 (Top-left)      8 (Top-center)      9 (Top-right)
4 (Middle-left)   5 (Middle-center)   6 (Middle-right)
1 (Bottom-left)   2 (Bottom-center)   3 (Bottom-right)
```

#### Position
```srt
{\pos(640,360)}Positioned at center of 1280×720 video
```

#### Other ASSA Tags
Some players support additional ASSA tags like `\fs` (font size), `\c` (color), `\bord` (border), etc. See [ASSA Override Tags](assa-override-tags.md) for details.

**Compatibility note:** ASS-style tags in SRT files have inconsistent support. Always test with your target player.

## Complete Example

```srt
1
00:00:01,000 --> 00:00:03,000
Normal text.

2
00:00:03,080 --> 00:00:05,080
Two
lines.

3
00:00:05,160 --> 00:00:07,160
<i>Italic</i>

4
00:00:07,240 --> 00:00:09,240
<b>Bold</b>

5
00:00:09,320 --> 00:00:11,320
<u>Underline</u>

6
00:00:11,400 --> 00:00:13,400
<font color="#ff0000">Red</font>

7
00:00:13,480 --> 00:00:15,480
{\an8}Top aligned

8
00:00:15,560 --> 00:00:17,560
<b><i>Bold and italic</i></b>

9
00:00:17,640 --> 00:00:19,640
<font color="#00ff00"><b>Bold green text</b></font>

10
00:00:19,720 --> 00:00:21,720
Line one
Line two
Line three
```

## Character Encoding

SubRip files should be saved as:
- **UTF-8 without BOM** (recommended, best compatibility)
- **UTF-8 with BOM** (acceptable, but BOM may cause issues in some players)
- **ANSI/Windows-1252** (legacy, avoid for non-ASCII characters)

**Best practice:** Always use UTF-8 without BOM for international character support.

## Line Breaks

- **Single line break** — Separate lines within the same subtitle (press Enter once)
- **Double line break** (blank line) — Separate subtitle blocks

**Example:**
```srt
1
00:00:01,000 --> 00:00:03,000
First line
Second line

2
00:00:03,000 --> 00:00:05,000
Next subtitle
```

## Common Issues and Solutions

### Issue: Timecodes not recognized
**Problem:** Using period (`.`) instead of comma (`,`) in timecodes
```
Wrong: 00:00:01.000 --> 00:00:03.000
Right: 00:00:01,000 --> 00:00:03,000
```

### Issue: Subtitles not appearing
**Problem:** Missing blank line between subtitle blocks
```
Wrong:
1
00:00:01,000 --> 00:00:03,000
Text one
2
00:00:03,000 --> 00:00:05,000
Text two

Right:
1
00:00:01,000 --> 00:00:03,000
Text one

2
00:00:03,000 --> 00:00:05,000
Text two
```

### Issue: HTML tags showing as text
**Problem:** Player doesn't support HTML formatting
**Solution:** Use a different player or convert to a format with better styling support (e.g., ASS)

### Issue: Special characters not displaying
**Problem:** Wrong character encoding
**Solution:** Save file as UTF-8 without BOM

### Issue: Subtitles out of sync
**Problem:** Incorrect timecodes or frame rate mismatch
**Solution:** Use Subtitle Edit's sync features or adjust FPS

## Best Practices

1. **Number sequentially:** Start at 1 and increment by 1
2. **No overlapping times:** End time of one subtitle should be before or equal to start time of next
3. **Use UTF-8 encoding:** Ensures international character support
4. **Keep text concise:** Aim for 2 lines maximum, 42 characters per line
5. **Reading time:** Allow at least 1-1.5 seconds per subtitle, longer for more text
6. **Blank lines:** Always separate subtitle blocks with a blank line
7. **Consistent formatting:** Use the same formatting style throughout
8. **Test compatibility:** Always test with your target player/device

## Reading Time Guidelines

Recommended display duration based on character count:

| Characters | Minimum Duration | Comfortable Duration |
|-----------|------------------|---------------------|
| 1-20 | 1.0 seconds | 1.5 seconds |
| 21-42 | 1.5 seconds | 2.5 seconds |
| 43-64 | 2.5 seconds | 3.5 seconds |
| 65-84 | 3.5 seconds | 4.5 seconds |

**Note:** These are guidelines. Adjust based on language, complexity, and target audience.

## Converting to SubRip

Subtitle Edit can convert many formats to SubRip:

1. Open your subtitle file
2. Go to **File** → **Save as...**
3. Select **SubRip (*.srt)** from format dropdown
4. Click **Save**

## Tools and Resources

### Recommended Tools
- **Subtitle Edit** — Full-featured subtitle editor with SRT support
- **Aegisub** — Advanced subtitle editor
- **Visual SubSync** — Subtitle synchronization tool
- **Subtitle Workshop** — Classic subtitle editor

### Validation
- Use Subtitle Edit's **Tools** → **Fix common errors** to validate SRT files
- Check for proper formatting and timing

### Online Resources
- [SubRip on VideoHelp](https://www.videohelp.com/software/Subrip) — Original tool
- [Matroska Subtitle Specification](https://www.matroska.org/technical/subtitles.html) — Technical details

## Technical Specifications

| Property | Value |
|----------|-------|
| **File Extension** | `.srt` |
| **MIME Type** | `application/x-subrip`, `text/srt` |
| **Character Encoding** | UTF-8 (recommended), UTF-8 with BOM, ANSI |
| **Line Endings** | Windows (CRLF), Unix (LF), or Mac (CR) |
| **Timecode Format** | `HH:MM:SS,mmm --> HH:MM:SS,mmm` |
| **Timecode Precision** | Milliseconds (3 digits) |
| **Maximum File Size** | No limit (typically a few KB to MB) |

## See Also

- [WebVTT Format](webvtt.md) — Modern HTML5 subtitle format with advanced features
- [ASSA Format](assa.md) — Advanced Sub Station Alpha format
- [ASSA Override Tags](assa-override-tags.md) — ASS-style tags reference
- [Supported Formats](supported-formats.md) — All formats supported by Subtitle Edit
- [Command Line](command-line.md) — Batch conversion with Subtitle Edit

## Version History

SubRip format has evolved over time:
- **2000** — Original SubRip format (plain text only)
- **2000s** — HTML-like tags added (italic, bold, underline)
- **2010s** — ASS-style override tags support in some players
- **Present** — Widely supported with varying levels of formatting support

**Note:** There is no official specification for SubRip. The format is defined by common practice and player implementations.
