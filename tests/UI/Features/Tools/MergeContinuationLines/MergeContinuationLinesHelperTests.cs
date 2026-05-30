using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;
using System.Collections.Generic;

namespace UITests.Features.Tools.MergeContinuationLines;

public class MergeContinuationLinesHelperTests
{
    private static SubtitleLineViewModel MakeSubtitle(string text, double startSec, double endSec) =>
        new()
        {
            Text = text,
            StartTime = TimeSpan.FromSeconds(startSec),
            EndTime = TimeSpan.FromSeconds(endSec),
        };

    [Fact]
    public void Detect_EnglishContinuationLines_FindsCandidates()
    {
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle("an", 1.00, 1.10),
            MakeSubtitle("illusion,", 1.12, 1.30),
            MakeSubtitle("an", 1.32, 1.40),
            MakeSubtitle("echo", 1.42, 1.60),
        };

        var candidates = MergeContinuationLinesHelper.Detect(subtitles, "en", maxGapMs: 500, maxTotalLength: 200);

        Assert.NotEmpty(candidates);
    }

    [Fact]
    public void Detect_MixedScriptFileDetectedAsJapanese_StillMergesEnglishBody()
    {
        // Regression for issue #11267: a few Japanese lines at the top make the
        // whole-file language detection return "ja", which previously suppressed
        // the entire feature ("No continuation candidates found"). CJK must be
        // skipped per line so the English continuation lines still merge.
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle("はよう", 0.62, 0.70),
            MakeSubtitle("はな", 0.81, 0.98),
            MakeSubtitle("an", 1.02, 1.13),
            MakeSubtitle("illusion,", 1.16, 1.74),
            MakeSubtitle("an", 1.80, 1.90),
            MakeSubtitle("echo", 1.92, 2.10),
        };

        var candidates = MergeContinuationLinesHelper.Detect(subtitles, "ja", maxGapMs: 500, maxTotalLength: 200);

        Assert.NotEmpty(candidates);
        // None of the produced candidates should start from a CJK (Japanese) line.
        Assert.DoesNotContain(candidates, c => c.Text1.Contains("はよう") || c.Text1.Contains("はな"));
    }

    [Fact]
    public void Detect_CjkLines_AreSkipped()
    {
        // Pure CJK continuation lines remain noise-free: ending in a CJK character
        // is not treated as an English-style "unfinished sentence".
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle("はよう", 0.62, 0.70),
            MakeSubtitle("はな", 0.81, 0.98),
        };

        var candidates = MergeContinuationLinesHelper.Detect(subtitles, "ja", maxGapMs: 500, maxTotalLength: 200);

        Assert.Empty(candidates);
    }
}
