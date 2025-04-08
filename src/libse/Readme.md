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
`libse` is licensed under the GNU LESSER GENERAL PUBLIC LICENSE Version 3, 
so it free to use for commercial software, as long as you don't modify the library itself. 
LGPL 3.0 allows linking to the library in a way that doesn't require you to open source your own code. 
This means that if you use libse in your project, you can keep your own code private, 
as long as you don't modify libse itself.