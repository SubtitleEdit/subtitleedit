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

## License
`libse` is licensed under the MIT License, so it is free to use for both personal and commercial software.
You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the library
without any restrictions, as long as the original copyright notice and license are included.