# Advanced Sub Station Alpha (ASSA) Format Reference

Advanced Sub Station Alpha (file extension `.ass`) is a powerful subtitle format that supports extensive text formatting, animations, and effects.

## Overview

The Sub Station Alpha (SSA) format was created by programmer [Kotus](https://web.archive.org/web/20030618072944/http://www.eswat.demon.co.uk/) in 1996 for the anime fansubbing community. In 2002, Advanced Sub Station Alpha (ASSA) was released with new features including layers, alpha channel support, and rotation.

Despite being called "Advanced Sub Station Alpha," the file extension remains `.ass` (though `.assa` would be more accurate).

**Key Features:**
- Powerful text formatting and styling
- Animation and transformation effects
- Multiple layers and styles
- Drawing and vector graphics support
- Precise positioning and timing
- Alpha channel (transparency) support
- 3D rotation effects
- Can be [embedded into Matroska (MKV) files](https://www.matroska.org/technical/subtitles.html)
- Supported by FFmpeg for burn-in

**Pros:**
- ✅ Extremely feature-rich
- ✅ Excellent support in players and converters
- ✅ Precise control over appearance and animation
- ✅ Multiple styles in one file
- ✅ Karaoke effects support

**Cons:**
- ❌ No new specification since [v4.00+](http://www.tcax.org/docs/ass-specs.htm) (2002)
- ❌ Limited number of full-featured editors
- ❌ Poorly chosen file extension
- ❌ Unintuitive BGR color format and inverted alpha values
- ❌ More complex than simple formats like SRT

## File Structure

An ASS file consists of several sections, each starting with a section header in square brackets:

1. **[Script Info]** — Metadata and script settings
2. **[V4+ Styles]** — Style definitions
3. **[Events]** — Subtitle lines and their properties

### Basic Structure

```
[Script Info]
Title: My Subtitle
ScriptType: v4.00+
...

[V4+ Styles]
Format: Name, Fontname, Fontsize, ...
Style: Default, Arial, 20, ...

[Events]
Format: Layer, Start, End, Style, ...
Dialogue: 0, 0:00:01.00, 0:00:03.00, Default, ...
```

## Timecode Format

ASS uses a different timecode format than SRT:

```
hours:minutes:seconds.centiseconds
```

**Important notes:**
- Hours use 1 digit (no zero-padding)
- Minutes and seconds use 2 digits (zero-padded)
- Centiseconds (hundredths of a second) use 2 digits
- Period (`.`) is used as decimal separator (not comma)

**Examples:**
```
0:00:01.00          (1 second)
0:01:15.25          (1 minute, 15.25 seconds)
1:30:45.10          (1 hour, 30 minutes, 45.1 seconds)
```

## [Script Info] Section

This section contains metadata and global settings for the subtitle file.

### Common Fields

```
[Script Info]
; This is an Advanced Sub Station Alpha v4+ script.
Title: My Subtitle File
ScriptType: v4.00+
WrapStyle: 0
PlayResX: 1920
PlayResY: 1080
ScaledBorderAndShadow: yes
Video Aspect Ratio: 0
Video Zoom: 6
Video Position: 0
```

### Important Fields

| Field | Description | Default |
|-------|-------------|---------|
| `Title` | Name of the subtitle file | (none) |
| `ScriptType` | Format version | `v4.00+` |
| `PlayResX` | Video width in pixels | `384` |
| `PlayResY` | Video height in pixels | `288` |
| `WrapStyle` | Line wrapping style (0-3) | `0` |
| `ScaledBorderAndShadow` | Scale borders with video | `yes` |
| `Timer` | Timer speed percentage | `100.0000` |

**Important:** Always set `PlayResX` and `PlayResY` to match your video resolution! The default of 384×288 is rarely correct for modern videos.

**WrapStyle values:**
- `0` — Smart wrapping, lines evenly balanced
- `1` — End-of-line wrapping
- `2` — No word wrapping
- `3` — Smart wrapping, lower line wider

## [V4+ Styles] Section

This section defines the appearance of subtitles.

### Format Line

The Format line defines the order and names of style fields:

```
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
```

### Style Line

Each style starts with `Style:` followed by comma-separated values matching the format:

```
Style: Default,Arial,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,1,1,2,10,10,10,1
```

### Style Fields Explained

| Field | Description | Example Values |
|-------|-------------|----------------|
| **Name** | Style name | `Default`, `Title`, `Sign` |
| **Fontname** | Font family | `Arial`, `Times New Roman` |
| **Fontsize** | Font size in points | `20`, `24`, `18` |
| **PrimaryColour** | Text color (BGR + Alpha) | `&H00FFFFFF` (white) |
| **SecondaryColour** | Secondary/karaoke color | `&H0000FFFF` (yellow) |
| **OutlineColour** | Border color | `&H00000000` (black) |
| **BackColour** | Shadow color | `&H00000000` (black) |
| **Bold** | Bold (0=no, -1=yes) | `0`, `-1` |
| **Italic** | Italic (0=no, -1=yes) | `0`, `-1` |
| **Underline** | Underline (0=no, -1=yes) | `0`, `-1` |
| **StrikeOut** | Strikeout (0=no, -1=yes) | `0`, `-1` |
| **ScaleX** | Horizontal scale (%) | `100`, `120` |
| **ScaleY** | Vertical scale (%) | `100`, `120` |
| **Spacing** | Letter spacing | `0`, `2` |
| **Angle** | Z-axis rotation (degrees) | `0`, `45` |
| **BorderStyle** | 1=outline+shadow, 3=box | `1`, `3` |
| **Outline** | Outline width | `1`, `2`, `0` |
| **Shadow** | Shadow depth | `1`, `2`, `0` |
| **Alignment** | Numpad alignment | `2` (bottom center) |
| **MarginL** | Left margin (pixels) | `10`, `20` |
| **MarginR** | Right margin (pixels) | `10`, `20` |
| **MarginV** | Vertical margin (pixels) | `10`, `20` |
| **Encoding** | Font encoding/charset | `1` (default) |

### Color Format

Colors in ASS are in **BGR** format (Blue-Green-Red, not RGB!) with alpha:

```
&H<AA><BB><GG><RR>
```

**Important notes:**
- Order is Blue, Green, Red (BGR), opposite of standard RGB
- Alpha values are **inverted**: `00` = opaque, `FF` = transparent
- Values are hexadecimal (00-FF)

**Color examples:**
```
&H00FFFFFF = White (opaque)
&H00000000 = Black (opaque)
&H000000FF = Red (opaque)
&H0000FF00 = Green (opaque)
&H00FF0000 = Blue (opaque)
&H8000FF00 = Green (50% transparent)
&HFF0000FF = Red (fully transparent/invisible)
```

### Alignment

Alignment uses numpad-style numbering:

```
7 (Top-left)      8 (Top-center)      9 (Top-right)
4 (Middle-left)   5 (Middle-center)   6 (Middle-right)
1 (Bottom-left)   2 (Bottom-center)   3 (Bottom-right)
```

Most subtitles use `2` (bottom-center) as default.

## [Events] Section

This section contains the actual subtitle lines.

### Format Line

```
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
```

### Dialogue Lines

```
Dialogue: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
```

**Example:**
```
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Hello, World!
```

### Event Fields

| Field | Description | Example |
|-------|-------------|---------|
| **Layer** | Rendering layer (0=bottom) | `0`, `1`, `2` |
| **Start** | Start timecode | `0:00:01.00` |
| **End** | End timecode | `0:00:03.00` |
| **Style** | Style name | `Default` |
| **Name** | Character name (optional) | `John`, `Narrator` |
| **MarginL** | Override left margin | `0`, `10` |
| **MarginR** | Override right margin | `0`, `10` |
| **MarginV** | Override vertical margin | `0`, `10` |
| **Effect** | Transition effect (rarely used) | `` |
| **Text** | Subtitle text with optional tags | `Hello, World!` |

### Comment Lines

Comments use `Comment:` instead of `Dialogue:`:

```
Comment: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,This line won't appear
```

## Override Tags

Override tags modify the appearance of text inline. They're enclosed in curly braces `{}`:

```
{\b1}Bold text{\b0} normal text
{\i1}Italic text{\i0} normal text
{\c&H0000FF&}Red text
```

See [ASSA Override Tags](assa-override-tags.md) for comprehensive documentation.

### Common Override Tags

| Tag | Description | Example |
|-----|-------------|---------|
| `\b1` / `\b0` | Bold on/off | `{\b1}Bold{\b0}` |
| `\i1` / `\i0` | Italic on/off | `{\i1}Italic{\i0}` |
| `\u1` / `\u0` | Underline on/off | `{\u1}Underline{\u0}` |
| `\s1` / `\s0` | Strikeout on/off | `{\s1}Strike{\s0}` |
| `\c&H<BGR>&` | Text color | `{\c&H0000FF&}Red` |
| `\fs<size>` | Font size | `{\fs30}Large text` |
| `\fn<name>` | Font name | `{\fnArial}Arial` |
| `\pos(x,y)` | Position | `{\pos(640,360)}Center` |
| `\an<1-9>` | Alignment | `{\an8}Top center` |
| `\fad(in,out)` | Fade in/out (ms) | `{\fad(500,500)}` |
| `\t(<tags>)` | Transform animation | `{\t(\frz360)}Rotate` |

## Line Breaks

- `\N` — Hard line break (always breaks)
- `\n` — Soft line break (breaks if needed)

**Example:**
```
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Line one\NLine two
```

## Complete Example

```
[Script Info]
; This is an Advanced Sub Station Alpha v4+ script.
Title: Example Subtitle
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080
ScaledBorderAndShadow: Yes

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,1,1,2,10,10,10,1
Style: Title,Arial,30,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,2,0,8,10,10,10,1
Style: Sign,Arial,18,&H00FFFF00,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,3,0,0,5,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Normal text.
Dialogue: 0,0:00:03.08,0:00:05.08,Default,,0,0,0,,Two\Nlines.
Dialogue: 0,0:00:05.16,0:00:07.16,Default,,0,0,0,,{\i1}Italic{\i0}
Dialogue: 0,0:00:07.24,0:00:09.24,Default,,0,0,0,,{\b1}Bold{\b0}
Dialogue: 0,0:00:09.32,0:00:11.32,Default,,0,0,0,,{\u1}Underline{\u0}
Dialogue: 0,0:00:11.40,0:00:13.40,Default,,0,0,0,,{\c&H0000FF&}Red
Dialogue: 0,0:00:13.48,0:00:15.48,Default,,0,0,0,,{\an8}Top aligned
Dialogue: 0,0:00:15.56,0:00:17.56,Title,,0,0,0,,Title style text
Dialogue: 1,0:00:10.00,0:00:20.00,Sign,,0,0,0,,{\pos(960,200)}On-screen sign
Comment: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,This is a comment
```

## Character Encoding

ASS files should be saved as:
- **UTF-8 without BOM** (recommended)
- **UTF-8 with BOM** (acceptable)
- **UTF-16** (less common)

**Best practice:** Always use UTF-8 without BOM for best compatibility.

## Layers

Layers control the rendering order of subtitles:
- **Layer 0** — Bottom layer (rendered first)
- **Layer 1, 2, 3...** — Higher layers (rendered on top)

Higher layer numbers appear on top of lower layer numbers. Useful for signs that should appear behind dialogue or complex typesetting.

## Comments in ASS Files

- **Semicolon comments:** Lines starting with `;` in [Script Info] section
- **Comment events:** Use `Comment:` instead of `Dialogue:` in [Events] section
- **Inline comments:** Can be added in override blocks (not recommended)

**Example:**
```
[Script Info]
; This is a comment
Title: My Subtitle

[Events]
Comment: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,This line is commented out
```

## Best Practices

1. **Set correct resolution:** Always set `PlayResX` and `PlayResY` to match your video
2. **Use UTF-8 encoding:** For international character support
3. **Name styles descriptively:** Use clear names like `Default`, `Signs`, `Narrator`
4. **Set ScaledBorderAndShadow:** Set to `yes` for resolution-independent borders
5. **Use layers wisely:** Keep dialogue on layer 0, signs on higher layers
6. **Test with target player:** ASS rendering varies between players
7. **Avoid excessive tags:** Too many override tags can impact performance
8. **Use comments:** Document complex styling or timing decisions

## Common Issues and Solutions

### Issue: Colors are wrong
**Problem:** Using RGB instead of BGR format
```
Wrong: &H00FF0000 (expecting red, but getting blue)
Right: &H000000FF (red in BGR format)
```

### Issue: Text is invisible
**Problem:** Alpha value set to FF (fully transparent)
```
Wrong: &HFF0000FF (fully transparent red)
Right: &H000000FF (opaque red)
```

### Issue: Positioning is off
**Problem:** PlayResX/PlayResY don't match video resolution
```
Wrong: PlayResX: 384, PlayResY: 288 (default)
Right: PlayResX: 1920, PlayResY: 1080 (for 1080p video)
```

### Issue: Borders don't scale
**Problem:** ScaledBorderAndShadow not set
```
Solution: Add to [Script Info]:
ScaledBorderAndShadow: yes
```

### Issue: Subtitles overlapping
**Problem:** Multiple subtitles with same layer and position
**Solution:** Use different layers or stagger timing

## Advanced Features

### Karaoke Effects
```
Dialogue: 0,0:00:00.00,0:00:05.00,Default,,0,0,0,,{\k50}Ka{\k50}ra{\k50}o{\k50}ke
```

### Drawing Mode
```
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,{\p1}m 0 0 l 100 0 100 100 0 100{\p0}
```

### Complex Animations
```
Dialogue: 0,0:00:01.00,0:00:05.00,Default,,0,0,0,,{\t(0,4000,\frz360\fscx200\fscy200)}Animated text
```

See [ASSA Override Tags](assa-override-tags.md) for comprehensive coverage of advanced features.

## Renderer Differences

Different players use different rendering engines:

- **libass** — Most common, used by mpv, VLC, MPC-HC
- **VSFilter / VSFilterMod** — Traditional renderer
- **xy-VSFilter** — Enhanced VSFilter with more features
- **XySubFilter** — Advanced renderer with more effects

**Note:** Some override tags may work differently or not at all depending on the renderer.

## Converting to ASS

Subtitle Edit can convert many formats to ASS:

1. Open your subtitle file
2. Go to **File** → **Save as...**
3. Select **Advanced Sub Station Alpha (*.ass)** from format dropdown
4. Click **Save**

## Tools and Resources

### Recommended Tools
- **Subtitle Edit** — Full-featured subtitle editor with ASS support
- **Aegisub** — Advanced subtitle editor, designed for ASS
- **ASSA Draw** — Vector drawing tool (built into Subtitle Edit)
- **mpv** — Video player with excellent libass support

### Libraries and Renderers
- [libass](https://github.com/libass/libass) — Portable subtitle renderer
- [VSFilter](https://github.com/HomeOfVapourSynthEvolution/VSFilterMod) — Classic renderer
- [xy-VSFilter](https://github.com/Cyberbeing/xy-VSFilter) — Enhanced renderer

### Documentation
- [ASS v4.00+ Specification](http://www.tcax.org/docs/ass-specs.htm) — Official spec (outdated)
- [Aegisub ASS Tags](http://docs.aegisub.org/3.2/ASS_Tags/) — Comprehensive tag reference
- [Matroska Subtitles](https://www.matroska.org/technical/subtitles.html) — Embedding ASS in MKV

## Technical Specifications

| Property | Value |
|----------|-------|
| **File Extension** | `.ass` |
| **MIME Type** | `text/x-ssa`, `application/x-ass` |
| **Character Encoding** | UTF-8 (recommended), UTF-16 |
| **Line Endings** | Windows (CRLF), Unix (LF) |
| **Timecode Format** | `H:MM:SS.cc` |
| **Timecode Precision** | Centiseconds (hundredths) |
| **Color Format** | BGR + inverted alpha |
| **Default Resolution** | 384×288 (should be overridden!) |

## See Also

- [ASSA Override Tags](assa-override-tags.md) — Comprehensive override tags reference
- [SubRip Format](subrip.md) — Simpler alternative format
- [WebVTT Format](webvtt.md) — Modern HTML5 subtitle format
- [Supported Formats](supported-formats.md) — All formats supported by Subtitle Edit
- [Command Line](command-line.md) — Batch conversion with Subtitle Edit

## Version History

- **1996** — Sub Station Alpha (SSA) v4 released by Kotus
- **2002** — Advanced Sub Station Alpha (ASS) v4.00+ released
  - Added layers
  - Added alpha channel support
  - Added rotation and 3D effects
- **2002-Present** — De facto standard maintained by libass and community

**Note:** The last official specification was v4.00+ in 2002. Modern features are defined by libass implementation and community practice.
