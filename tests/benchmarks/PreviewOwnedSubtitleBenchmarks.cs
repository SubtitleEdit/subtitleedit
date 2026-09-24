using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Models the subtitle snapshot a video preview refresh takes (MainViewModel.GetVideoPreviewSubtitle
/// + MpvReloader.RefreshMpv / VlcReloader.RefreshVlc). The old shape rebuilt the live working
/// subtitle from the grid rows (GetUpdateSubtitle) and the reloader then deep-copied it, because
/// the live instance can be cleared by other UI code during the background serialize. The new
/// shape builds a throw-away subtitle the preview owns, so the reloader skips the copy.
/// Both shapes are modeled here so the win is visible in a single run.
/// </summary>
[MemoryDiagnoser]
public class PreviewOwnedSubtitleBenchmarks
{
    private readonly SubtitleFormat _format = new SubRip();
    private List<SubtitleLineViewModel> _rows = new();
    private Subtitle _live = new();

    [Params(1000, 5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rows = SubtitleFactory.Make(Lines);
        _live = new Subtitle { Header = string.Empty, FileName = "movie.srt" };

        var oldShape = LiveSnapshotPlusDeepCopy();
        var newShape = OwnedSnapshot();
        if (oldShape.Paragraphs.Count != newShape.Paragraphs.Count ||
            oldShape.GetFastHashCode(null) != newShape.GetFastHashCode(null))
        {
            throw new InvalidOperationException("Shapes differ");
        }
    }

    [Benchmark(Baseline = true)]
    public Subtitle LiveSnapshotPlusDeepCopy()
    {
        // Old: GetUpdateSubtitle() refills the live _subtitle...
        _live.Paragraphs.Clear();
        foreach (var line in _rows)
        {
            if (line.IsReferenceOnly)
            {
                continue;
            }

            _live.Paragraphs.Add(line.ToParagraph(_format));
        }

        // ...and RefreshMpv/RefreshVlc deep-copy it before the background serialize.
        return new Subtitle(_live, false);
    }

    [Benchmark]
    public Subtitle OwnedSnapshot()
    {
        // New: GetVideoPreviewSubtitle() builds a subtitle only the preview holds.
        var subtitle = new Subtitle
        {
            Header = _live.Header,
            Footer = _live.Footer,
            OriginalFormat = _live.OriginalFormat,
            FileName = _live.FileName,
        };
        subtitle.Paragraphs.Capacity = _rows.Count;

        foreach (var line in _rows)
        {
            if (!line.IsReferenceOnly)
            {
                subtitle.Paragraphs.Add(line.ToParagraph(_format));
            }
        }

        return subtitle;
    }
}
