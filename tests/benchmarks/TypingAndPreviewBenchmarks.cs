using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Models the video preview refresh snapshot in MpvReloader.RefreshMpv for a non-ASSA UI
/// format (SubRip - the default): the old shape deep-copied the already-private snapshot a
/// second time before swapping in the preview style header; the new shape mutates the
/// private copy in place and only keeps the original header string for the STL checks.
/// Both shapes are modeled here so the win is visible in a single run.
/// </summary>
[MemoryDiagnoser]
public class PreviewSnapshotBenchmarks
{
    private Subtitle _subtitle = new();
    private string _previewHeader = string.Empty;

    [Params(1000, 5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var texts = new[]
        {
            "It was the best of times, it was the worst of times.",
            "<i>Are you coming with us?</i>",
            "- No." + Environment.NewLine + "- Then stay here and wait.",
            "1999 was a very long time ago.",
            "Hello.",
        };

        _subtitle = new Subtitle();
        var startMs = 1000.0;
        for (var i = 0; i < Lines; i++)
        {
            _subtitle.Paragraphs.Add(new Paragraph(texts[i % texts.Length], startMs, startMs + 2000));
            startMs += 2100;
        }

        _previewHeader = string.Format(
            Nikse.SubtitleEdit.Core.SubtitleFormats.AdvancedSubStationAlpha.HeaderNoStyles,
            "MPV preview file",
            new SsaStyle().ToRawAss(SsaStyle.DefaultAssStyleFormat));
    }

    [Benchmark(Baseline = true)]
    public int SecondDeepCopy()
    {
        // Old RefreshMpv shape: private snapshot copy + a second full deep copy.
        var snapshot = new Subtitle(_subtitle, false);
        var oldSub = snapshot;
        var working = new Subtitle(snapshot);
        working.Header = _previewHeader;
        return working.Paragraphs.Count + (oldSub.Header?.Length ?? 0);
    }

    [Benchmark]
    public int MutateSnapshotInPlace()
    {
        // New RefreshMpv shape: keep the original header string, mutate the private copy.
        var snapshot = new Subtitle(_subtitle, false);
        var oldHeader = snapshot.Header;
        snapshot.Header = _previewHeader;
        return snapshot.Paragraphs.Count + (oldHeader?.Length ?? 0);
    }
}

/// <summary>
/// Models the per-keystroke text-info work in the edit box: the row view model's bindings
/// (text error tint) strip + split the text once via their memo, and the old
/// SubtitleTextInfoHelper.Populate stripped + split the very same text a second time for the
/// line-length panel. The new Populate reuses the memo, so only one strip + split remains.
/// </summary>
[MemoryDiagnoser]
public class KeystrokeStripSplitBenchmarks
{
    private string[] _texts = [];

    [GlobalSetup]
    public void Setup()
    {
        // Rotating texts model successive keystroke states; ASSA override tags and italics
        // make the strip do real work, like the issue reporter's ASSA project.
        _texts =
        [
            @"{\an8}It was the best of times,",
            @"{\an8}It was the best of times, it",
            @"{\an8}It was the best of times, it was",
            "<i>Are you coming\r\nwith us?</i>",
            "<i>Are you coming\r\nwith us now?</i>",
            @"{\i1}- No.{\i0}" + "\r\n" + "- Then stay here.",
        ];
    }

    [Benchmark(Baseline = true)]
    public int StripAndSplitTwice()
    {
        var total = 0;
        foreach (var text in _texts)
        {
            // Row view model memo build (tint/pixel-width bindings).
            var stripped1 = HtmlUtil.RemoveHtmlTags(text, true);
            total += stripped1.SplitToLines().Count;

            // Old Populate: same strip + split again for the line-length panel.
            var stripped2 = HtmlUtil.RemoveHtmlTags(text, true);
            total += stripped2.SplitToLines().Count;
        }

        return total;
    }

    [Benchmark]
    public int StripAndSplitOnce()
    {
        var total = 0;
        foreach (var text in _texts)
        {
            // New Populate reuses the row view model's memo, so each keystroke pays one
            // strip + split; the second consumer just reads the cached values.
            var stripped = HtmlUtil.RemoveHtmlTags(text, true);
            var lines = stripped.SplitToLines();
            total += lines.Count;
            total += lines.Count;
        }

        return total;
    }
}
