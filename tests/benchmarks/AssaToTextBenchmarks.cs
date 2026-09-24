using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// AdvancedSubStationAlpha.ToText - runs on every mpv/vlc preview refresh after an edit and on
/// every ASSA save. Run before/after the StringBuilder sizing change; FastHash is the control.
/// </summary>
[MemoryDiagnoser]
public class AssaToTextBenchmarks
{
    private readonly AdvancedSubStationAlpha _ass = new();
    private Subtitle _subtitle = new();

    [Params(100, 1000, 5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var texts = new[]
        {
            "It was the best of times, it was the worst of times.",
            "{\\i1}Are you coming with us?{\\i0}",
            "- No." + Environment.NewLine + "- Then stay here and wait.",
            "I told you already, this is not going to work out the way you think it will.",
            "Hello.",
        };

        // The mpv preview header (MpvReloader.UpdateMpvStyle).
        _subtitle = new Subtitle
        {
            Header = string.Format(AdvancedSubStationAlpha.HeaderNoStyles, "MPV preview file",
                new SsaStyle().ToRawAss(SsaStyle.DefaultAssStyleFormat)),
        };

        var startMs = 1000.0;
        for (var i = 0; i < Lines; i++)
        {
            _subtitle.Paragraphs.Add(new Paragraph(texts[i % texts.Length], startMs, startMs + 2000) { Extra = "Default" });
            startMs += 2100;
        }
    }

    [Benchmark]
    public string ToText() => _subtitle.ToText(_ass);

    [Benchmark]
    public int FastHash() => _subtitle.GetFastHashCode(null);
}
