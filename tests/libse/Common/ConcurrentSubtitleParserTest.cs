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

        var unknown = Path.Combine(_folder, "unknown.txt");
        File.WriteAllText(unknown, "just some text" + Environment.NewLine + "not a subtitle");
        fileNames.Add(unknown);
        return fileNames;
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
            string expected;
            using (ThreadFrameRateScope.Begin())
            {
                expected = Describe(Subtitle.Parse(fileName));
            }

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
            var expected = fileNames.ToDictionary(f => f, f =>
            {
                using (ThreadFrameRateScope.Begin())
                {
                    return Describe(Subtitle.Parse(f));
                }
            });

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
    public void ThreadFrameRateScopeIsPerThreadAndRestores()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            using (ThreadFrameRateScope.Begin())
            {
                Assert.Equal(25, Configuration.Settings.General.CurrentFrameRate);
                Configuration.Settings.General.CurrentFrameRate = 30;
                Assert.Equal(30, Configuration.Settings.General.CurrentFrameRate);

                double otherThread = 0;
                var thread = new Thread(() => otherThread = Configuration.Settings.General.CurrentFrameRate);
                thread.Start();
                thread.Join();
                Assert.Equal(25, otherThread);

                using (ThreadFrameRateScope.Begin())
                {
                    Configuration.Settings.General.CurrentFrameRate = 50;
                }

                Assert.Equal(30, Configuration.Settings.General.CurrentFrameRate);
            }

            Assert.Equal(25, Configuration.Settings.General.CurrentFrameRate);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }
}
