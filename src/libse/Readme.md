# libse

`libse` is the subtitle engine behind [Subtitle Edit](https://github.com/SubtitleEdit/subtitleedit) —
a general-purpose .NET library for reading, writing, converting, and fixing subtitles.

- **300+ subtitle formats**: SubRip (.srt), Advanced SubStation Alpha (.ass), WebVTT (.vtt),
  SAMI, EBU STL, PAC, Cavena 890, Timed Text / TTML, DFXP, SCC, and many more
- **Image-based and container formats**: Blu-ray SUP, VobSub (.sub/.idx), DVB subtitles in
  transport streams, Matroska (.mkv), MP4, MXF
- **Fixing and formatting**: fix common errors, auto-break/unbreak lines, remove text for
  hearing impaired, remove interjections, merge/split lines, bridge gaps, beautify time codes
- **Detection**: subtitle format auto-detection, text encoding detection, language auto-detection
- Targets **.NET Standard 2.1** and **.NET 10**, MIT licensed

## Install

```
dotnet add package libse
```

## Namespaces

```csharp
using Nikse.SubtitleEdit.Core.Common;          // Subtitle, Paragraph, Utilities, HtmlUtil, ...
using Nikse.SubtitleEdit.Core.SubtitleFormats; // SubRip, WebVTT, Ebu, ... and SubtitleFormat
```

## Load a subtitle file

The format and text encoding are detected automatically:

```csharp
var subtitle = Subtitle.Parse(fileName);
if (subtitle == null)
{
    return; // not a known subtitle format
}

var numberOfLines = subtitle.Paragraphs.Count;
var firstText = subtitle.Paragraphs.First().Text;
var firstStartMs = subtitle.Paragraphs.First().StartTime.TotalMilliseconds;
var formatName = subtitle.OriginalFormat.FriendlyName; // e.g. "SubRip (.srt)"
var encoding = subtitle.OriginalEncoding;
```

`Subtitle.Parse` handles text formats and the binary formats in `SubtitleFormat.AllSubtitleFormats`
(e.g. EBU STL, PAC, Cavena 890). Image-based and container formats have their own parsers:

| Format | Parser |
| --- | --- |
| Blu-ray SUP | `BluRaySup.BluRaySupParser.ParseBluRaySup` |
| VobSub (.sub/.idx) | `VobSub.VobSubParser` |
| Matroska (.mkv) | `ContainerFormats.Matroska.MatroskaFile` |
| MP4 | `ContainerFormats.Mp4.Mp4Parser` |
| MXF | `ContainerFormats.MaterialExchangeFormat.MxfParser` |
| Transport stream (DVB) | `ContainerFormats.TransportStream.TransportStreamParser` |

```csharp
using Nikse.SubtitleEdit.Core.BluRaySup;

var log = new StringBuilder();
foreach (var image in BluRaySupParser.ParseBluRaySup("movie.sup", log))
{
    var start = image.StartTimeCode;
    var end = image.EndTimeCode;
    using (var bitmap = image.GetBitmap()) // SkiaSharp SKBitmap
    {
        // OCR or save the bitmap
    }
}
```

## Save / convert a subtitle file

Text formats serialize a `Subtitle` via `ToText`, so converting is just loading with one
format and saving with another:

```csharp
File.WriteAllText("new.srt", new SubRip().ToText(subtitle, "untitled"));
File.WriteAllText("new.vtt", new WebVTT().ToText(subtitle, "untitled"));
```

Binary formats (EBU STL, PAC, Cavena 890, ...) do not support `ToText`. They implement
`IBinaryPersistableSubtitle` and are written with `Save`:

```csharp
new Ebu().Save("new.stl", subtitle);
```

All formats are available via `SubtitleFormat.AllSubtitleFormats`.

## Common operations

```csharp
// Shift all cues 1.5 seconds later
subtitle.AddTimeToAllParagraphs(TimeSpan.FromSeconds(1.5));

// Convert from 25 fps to 23.976 fps (scales all times by 25 / 23.976)
subtitle.ChangeFrameRate(25.0, 23.976);

// Auto-break a long line into two balanced lines
var broken = Utilities.AutoBreakLine("This is a very long subtitle line that should be broken into two nicely balanced lines");

// Strip formatting
var plain = HtmlUtil.RemoveHtmlTags(text, alsoSsaTags: true);

// Detect the language of a subtitle
var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // e.g. "en"
```

## License

`libse` is licensed under the MIT License, so it is free to use for both personal and commercial software.
You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the library
without any restrictions, as long as the original copyright notice and license are included.
