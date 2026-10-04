using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 30. Round30SaveBenchmarks calls the real ToText, so it is run once against the commit
// before the change and once after; ControlAutoBreak is the drift control.

/// <summary>Saving D-Cinema SMPTE and Final Cut Pro XML (a third of the lines italic).</summary>
[MemoryDiagnoser]
public class Round30SaveBenchmarks
{
    private static readonly string[] Texts =
    {
        "It was the best of times {0}, it was the worst.",
        "<i>I told you already {0}.</i>",
        "- No, Mrs. smith {0}.\r\n- Then <i>stay</i> here & wait.",
        "<i>Somewhere in Denmark {0},\r\na quiet evening.</i>",
        "Plain line {0} with <b>bold</b>",
        "Tom & \"Jerry\" > {0} <",
    };

    private Subtitle _subtitle = null!;

    [Params(2000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = new Subtitle();
        for (var i = 0; i < Lines; i++)
        {
            _subtitle.Paragraphs.Add(new Paragraph(string.Format(Texts[i % Texts.Length], i), i * 2500, i * 2500 + 2000));
        }
    }

    [Benchmark]
    public int SaveDCinemaSmpte2014() => new DCinemaSmpte2014().ToText(_subtitle, "title").Length;

    [Benchmark]
    public int SaveFinalCutProXml() => new FinalCutProXml().ToText(_subtitle, "title").Length;

    /// <summary>What "Save as" no longer runs when appending the language code is off (the default).</summary>
    [Benchmark]
    public string? SaveAsLanguageDetect() => LanguageAutoDetect.AutoDetectGoogleLanguageOrNull2(_subtitle);

    /// <summary>Drift control - untouched by this round.</summary>
    [Benchmark]
    public int ControlAutoBreak()
    {
        var total = 0;
        for (var i = 0; i < 3000; i++)
        {
            total += Utilities.AutoBreakLine(BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length]).Length;
        }

        return total;
    }
}

/// <summary>
/// Timeline tracks render after "select all": old = <c>List.Contains</c> per visible block,
/// new = a set of the selected blocks inside the visible window (as the waveform does).
/// </summary>
[MemoryDiagnoser]
public class Round30TimelineSelectionBenchmarks
{
    private List<SubtitleLineViewModel> _all = null!;
    private readonly HashSet<SubtitleLineViewModel> _set = new();
    private const double StartSeconds = 10000;
    private const double PixelsPerSecond = 20;
    private const double Width = 2000; // 100 seconds visible, ~50 blocks

    [Params(10000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _all = BenchmarkSubtitles.Build(Lines).Paragraphs.Select(p => new SubtitleLineViewModel(p, new SubRip())).ToList();
        if (Old() != New())
        {
            throw new InvalidOperationException("Different selection");
        }
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var selected = _all; // select all
        var count = 0;
        foreach (var paragraph in _all)
        {
            var x1 = (paragraph.StartTime.TotalSeconds - StartSeconds) * PixelsPerSecond;
            var x2 = (paragraph.EndTime.TotalSeconds - StartSeconds) * PixelsPerSecond;
            if (x2 < 0 || x1 > Width)
            {
                continue;
            }

            if (selected.Contains(paragraph))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int New()
    {
        var selected = _all; // select all
        _set.Clear();
        foreach (var paragraph in selected)
        {
            var x1 = (paragraph.StartTime.TotalSeconds - StartSeconds) * PixelsPerSecond;
            var x2 = (paragraph.EndTime.TotalSeconds - StartSeconds) * PixelsPerSecond;
            if (!(x2 < 0 || x1 > Width))
            {
                _set.Add(paragraph);
            }
        }

        var count = 0;
        foreach (var paragraph in _all)
        {
            var x1 = (paragraph.StartTime.TotalSeconds - StartSeconds) * PixelsPerSecond;
            var x2 = (paragraph.EndTime.TotalSeconds - StartSeconds) * PixelsPerSecond;
            if (x2 < 0 || x1 > Width)
            {
                continue;
            }

            if (_set.Contains(paragraph))
            {
                count++;
            }
        }

        return count;
    }
}
