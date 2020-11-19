# libse

## How to load a subtitle file
```csharp
var subtitle = new Subtitle();
var subRip = new SubRip();
var lines = File.ReadAllLines(@"C:\test.srt").ToList();
subRip.LoadSubtitle(subtitle, lines, "untitled");
var numberOfSubtitleLines = subtitle.Paragraphs.Count;
var firstText = subtitle.Paragraphs.First().Text;
var firstStartMilliseconds = subtitle.Paragraphs.First().StartTime.TotalMilliseconds;
```

## How to save a subtitle file
```csharp
File.WriteAllText(@"C:\Data\new.srt", subRip.ToText(subtitle, "untitled"));
```