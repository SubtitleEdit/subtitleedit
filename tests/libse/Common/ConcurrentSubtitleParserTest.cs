using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Common;

[Collection("NonParallelTests")]
public class ConcurrentSubtitleParserTest : IDisposable
{
    private readonly string _folder;

    public ConcurrentSubtitleParserTest()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se_concurrent_parse_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // best effort
        }
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < 25; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph($"Line {i} text" + Environment.NewLine + $"second {i}", 1000 + i * 3000, 3000 + i * 3000));
        }

        return subtitle;
    }

    private List<string> WriteSamples()
    {
        var subtitle = MakeSubtitle();
        var formats = new SubtitleFormat[]
        {
            new SubRip(),
            new AdvancedSubStationAlpha(),
            new SubStationAlpha(),
            new WebVTT(),
            new MicroDvd(), // frame based - reads CurrentFrameRate
            new TimedText10(), // writes the frame rate it declares into CurrentFrameRate
            new DCinemaSmpte2014(), // ditto
            new YouTubeSbv(),
        };

        // Files that declare their own frame rate (TTML, D-Cinema) are written at another rate
        // than the 25 fps the frame-based ones are read at, so a frame rate leaking between
        // parallel parses changes the result.
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        var fileNames = new List<string>();
        try
        {
            foreach (var format in formats)
            {
                Configuration.Settings.General.CurrentFrameRate = format is TimedText10 ? 30 : format is DCinemaSmpte2014 ? 24 : 25;
                var fileName = Path.Combine(_folder, format.GetType().Name + format.Extension);
                File.WriteAllText(fileName, format.ToText(subtitle, "test"));
                fileNames.Add(fileName);
            }
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }

        // TTML with its own frame rate and frame time codes: 15 frames is 500 ms at 30 fps,
        // 600 ms at 25 - so it only loads right with its own rate.
        var ttmlFrames = Path.Combine(_folder, "frames30.xml");
        File.WriteAllText(ttmlFrames,
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
            "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" ttp:timeBase=\"smpte\" ttp:frameRate=\"30\" xml:lang=\"en\">" + Environment.NewLine +
            "  <body><div>" + Environment.NewLine +
            "    <p begin=\"00:00:01:15\" end=\"00:00:03:15\">One</p>" + Environment.NewLine +
            "    <p begin=\"00:00:04:15\" end=\"00:00:06:00\">Two</p>" + Environment.NewLine +
            "  </div></body>" + Environment.NewLine +
            "</tt>");
        fileNames.Add(ttmlFrames);

        var unknown = Path.Combine(_folder, "unknown.txt");
        File.WriteAllText(unknown, "just some text" + Environment.NewLine + "not a subtitle");
        fileNames.Add(unknown);
        return fileNames;
    }

    /// <summary>
    /// Plain Subtitle.Parse starting at <paramref name="frameRate"/> - what every parallel parse
    /// should match. The global frame rate is restored after, as a file can declare its own.
    /// </summary>
    private static string ParseSequential(string fileName, double frameRate)
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = frameRate;
            return Describe(Subtitle.Parse(fileName));
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    private static string Describe(Subtitle? subtitle)
    {
        if (subtitle == null)
        {
            return "null";
        }

        return subtitle.OriginalFormat.Name + "|" + subtitle.OriginalEncoding?.WebName + "|" +
               string.Join(";", subtitle.Paragraphs.Select(p => $"{p.StartTime.TotalMilliseconds}-{p.EndTime.TotalMilliseconds}:{p.Text}"));
    }

    [Fact]
    public void ParseMatchesSubtitleParse()
    {
        foreach (var fileName in WriteSamples())
        {
            var expected = ParseSequential(fileName, Configuration.Settings.General.CurrentFrameRate);
            Assert.Equal(expected, Describe(ConcurrentSubtitleParser.Parse(fileName)));
        }
    }

    [Fact]
    public void ParallelParseMatchesSequentialParse()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var fileNames = WriteSamples();
            // Each file on its own, starting at 25 fps - what every parallel parse should see
            var expected = fileNames.ToDictionary(f => f, f => ParseSequential(f, 25));

            var work = Enumerable.Range(0, 40).SelectMany(_ => fileNames).ToList();
            var actual = new string[work.Count];
            Parallel.For(0, work.Count, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
            {
                actual[i] = Describe(ConcurrentSubtitleParser.Parse(work[i]));
            });

            for (var i = 0; i < work.Count; i++)
            {
                Assert.Equal(expected[work[i]], actual[i]);
            }
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Fact]
    public void CommonFormatsDoNotUseTheSharedInstance()
    {
        var fileName = WriteSamples()[0]; // SubRip
        var subtitle = ConcurrentSubtitleParser.Parse(fileName);

        Assert.NotNull(subtitle);
        Assert.IsType<SubRip>(subtitle.OriginalFormat);
        Assert.DoesNotContain(SubtitleFormat.AllSubtitleFormats, f => ReferenceEquals(f, subtitle.OriginalFormat));
    }

    [Fact]
    public void ParseDoesNotChangeGlobalFrameRate()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            foreach (var fileName in WriteSamples())
            {
                ConcurrentSubtitleParser.Parse(fileName);
            }

            Assert.Equal(23.976, Configuration.Settings.General.CurrentFrameRate);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Fact]
    public void DeclaredFrameRateStaysOnTheFormatInstance()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var ttml = WriteSamples().Single(f => f.EndsWith("frames30.xml"));

            var subtitle = ConcurrentSubtitleParser.Parse(ttml);
            Assert.NotNull(subtitle);
            Assert.Equal(30, subtitle.OriginalFormat.DeclaredFrameRate);
            Assert.Equal(1500, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(25, Configuration.Settings.General.CurrentFrameRate);

            // File > Open still takes the file's frame rate, as before
            Assert.NotNull(Subtitle.Parse(ttml));
            Assert.Equal(30, Configuration.Settings.General.CurrentFrameRate);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    [Fact]
    public void UnknownFileReturnsTheLinesRead()
    {
        var unknown = WriteSamples().Single(f => f.EndsWith("unknown.txt"));

        var subtitle = ConcurrentSubtitleParser.Parse(unknown, out var lines);

        Assert.Null(subtitle);
        Assert.NotNull(lines);
        Assert.Equal(new[] { "just some text", "not a subtitle" }, lines);
    }

    [Theory]
    [InlineData(".srt")]
    [InlineData(".ass")]
    public void FileLoadRetainsParsedContentAndDiagnostics(string extension)
    {
        SubtitleFormat reader = extension == ".srt" ? new SubRip() : new AdvancedSubStationAlpha();
        var input = reader.ToText(MakeSubtitle(), "sample");
        input = extension == ".srt"
            ? "This line is outside any cue" + Environment.NewLine + input
            : input + Environment.NewLine + "Dialogue: 0,bad,0:00:02.00,Default,,0,0,0,,Broken" +
              Environment.NewLine + "[Fonts]" + Environment.NewLine + "fontname: embedded.ttf";

        var path = Path.Combine(_folder, "sample" + extension);
        File.WriteAllText(path, input);

        var expected = new Subtitle { Header = null! };
        reader.LoadSubtitle(expected, input.SplitToLines(), path);
        var loaded = Subtitle.Parse(path);
        Assert.NotNull(loaded);
        Assert.IsType(reader.GetType(), loaded.OriginalFormat);
        Assert.Equal(expected.Header, loaded.Header);
        Assert.Equal(expected.Footer, loaded.Footer);
        Assert.Equal(expected.Paragraphs.Select(p => (p.Number, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, p.Text)),
            loaded.Paragraphs.Select(p => (p.Number, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, p.Text)));

        static string ReadErrors(SubtitleFormat format) => format is SubRip srt ? srt.Errors : ((AdvancedSubStationAlpha)format).Errors;
        Assert.NotEmpty(ReadErrors(reader));
        Assert.Equal(ReadErrors(reader), ReadErrors(loaded.OriginalFormat));

        var detectOnly = new Subtitle();
        var detected = detectOnly.LoadSubtitle(path, out _, null, batchMode: false, loadSubtitle: false);
        Assert.IsType(reader.GetType(), detected);
        Assert.Empty(detectOnly.Paragraphs);
        Assert.Null(ReadErrors(detected!));
    }

    [Fact]
    public void RenamedWebVttFallsBackFromSubRip()
    {
        var path = Path.Combine(_folder, "renamed.srt");
        File.WriteAllText(path, "WEBVTT\n\n00:01.000 --> 00:02.000\nCaption\n");
        var subtitle = Subtitle.Parse(path);
        Assert.NotNull(subtitle);
        Assert.IsType<WebVTT>(subtitle.OriginalFormat);
        Assert.Equal("Caption", Assert.Single(subtitle.Paragraphs).Text);
    }

    [Fact]
    public void MissingFileReturnsNoLines()
    {
        var subtitle = ConcurrentSubtitleParser.Parse(Path.Combine(_folder, "does-not-exist.srt"), out var lines);

        Assert.Null(subtitle);
        Assert.Null(lines);
    }
}
