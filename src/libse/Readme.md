# libse

## How to load a subtitle file
```csharp
var subtitle = Subtitle.Parse(fileName);
var numberOfSubtitleLines = subtitle.Paragraphs.Count;
var firstText = subtitle.Paragraphs.First().Text;
var firstStartMilliseconds = subtitle.Paragraphs.First().StartTime.TotalMilliseconds;
```

## How to save a subtitle file
```csharp
File.WriteAllText(@"C:\Data\new.srt", new SubRip().ToText(subtitle, "untitled"));
```