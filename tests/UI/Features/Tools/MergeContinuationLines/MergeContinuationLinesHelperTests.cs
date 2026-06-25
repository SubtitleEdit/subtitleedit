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
    public void Detect_CjkContinuationLines_MergeLikeEnglish()
    {
        // Issue #11806: CJK lines must be treated just like English. A CJK line that
        // does not end in sentence-final punctuation is an unfinished sentence and
        // should merge with the next line.
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle("これは", 0.62, 0.70),
            MakeSubtitle("ペンです", 0.81, 0.98),
        };

        var candidates = MergeContinuationLinesHelper.Detect(subtitles, "ja", maxGapMs: 500, maxTotalLength: 200);

        Assert.NotEmpty(candidates);
    }

    [Fact]
    public void Detect_CjkSentenceEndingWithIdeographicFullStop_NotMerged()
    {
        // The ideographic full stop "。" ends a sentence, so the line is not a
        // continuation and must not be offered as a merge candidate.
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle("これはペンです。", 0.62, 0.70),
            MakeSubtitle("そうですか", 0.81, 0.98),
        };

        var candidates = MergeContinuationLinesHelper.Detect(subtitles, "ja", maxGapMs: 500, maxTotalLength: 200);

        Assert.Empty(candidates);
    }
}
