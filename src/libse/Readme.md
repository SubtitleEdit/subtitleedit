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

## Load a subtitle file

The format and text encoding are detected automatically:

```csharp
var subtitle = Subtitle.Parse(fileName); // null if not a known subtitle format
var numberOfLines = subtitle.Paragraphs.Count;
var firstText = subtitle.Paragraphs.First().Text;
var firstStartMs = subtitle.Paragraphs.First().StartTime.TotalMilliseconds;
var formatName = subtitle.OriginalFormat.FriendlyName; // e.g. "SubRip (.srt)"
```

## Save / convert a subtitle file

Every format can serialize a `Subtitle` via `ToText`, so converting is just loading with one
format and saving with another:

```csharp
File.WriteAllText("new.srt", new SubRip().ToText(subtitle, "untitled"));
File.WriteAllText("new.vtt", new WebVTT().ToText(subtitle, "untitled"));
```

All formats are available via `SubtitleFormat.AllSubtitleFormats`.

## Common operations

```csharp
// Shift all cues 1.5 seconds later
subtitle.AddTimeToAllParagraphs(TimeSpan.FromSeconds(1.5));

// Re-time frame-based cues after a frame rate change
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
