using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Every Paragraph constructor generates an id, so its cost is paid per line on every
/// GetUpdateSubtitle, video preview refresh and file load. Run before/after the id change;
/// FastHash creates no paragraphs and is the drift control.
/// </summary>
[MemoryDiagnoser]
public class ParagraphIdBenchmarks
{
    private readonly SubtitleFormat _srt = new SubRip();
    private List<SubtitleLineViewModel> _rows = new();
    private List<string> _srtLines = new();
    private Subtitle _subtitle = new();

    [Params(1000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rows = SubtitleFactory.Make(Lines);
        _subtitle = new Subtitle();
        foreach (var row in _rows)
        {
            _subtitle.Paragraphs.Add(row.ToParagraph(_srt));
        }

        _srtLines = _subtitle.ToText(_srt).SplitToLines();
    }

    [Benchmark]
    public int RowsToParagraphs()
    {
        var n = 0;
        foreach (var row in _rows)
        {
            n += row.ToParagraph(_srt).Number;
        }

        return n;
    }

    [Benchmark]
    public int LoadSubRip()
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, _srtLines, null);
        return subtitle.Paragraphs.Count;
    }

    [Benchmark]
    public int DeepCopy() => new Subtitle(_subtitle).Paragraphs.Count;

    [Benchmark]
    public int FastHash() => _subtitle.GetFastHashCode(null);
}
