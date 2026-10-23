using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;
using Nikse.SubtitleEdit.UiLogic.MergeContinuationLines;

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

    [Theory]
    [InlineData("Ich weiß", "nicht, was du meinst.")] // ß is not in the configured alphabet
    [InlineData("Wir treffen uns um 10", "Uhr am Bahnhof.")]
    [InlineData("Er sagte:", "Komm sofort nach Hause.")]
    [InlineData("Er sagte \"komm\"", "und ging dann weg.")]
    [InlineData("Er schrie „Hilfe“", "und rannte weg.")]
    [InlineData("<i>Das ist die Straße</i>", "<i>nach Hause.</i>")]
    [InlineData("זה לא", "מה שחשבתי.")]
    [InlineData("I was going to say...", "never mind.")]
    public void Detect_LineWithoutSentenceEnding_IsMerged(string text1, string text2)
    {
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle(text1, 1.00, 2.00),
            MakeSubtitle(text2, 2.10, 3.00),
        };

        var candidates = MergeContinuationLinesHelper.Detect(subtitles, "de", maxGapMs: 500, maxTotalLength: 200);

        Assert.Single(candidates);
    }

    [Theory]
    [InlineData("Das ist gut.", "Ja, sehr gut.")]
    [InlineData("Wirklich?", "Ja.")]
    [InlineData("Hör auf!", "Sofort.")]
    [InlineData("Er sagte \"komm.\"", "Dann ging er.")]
    [InlineData("(lacht)", "Das ist lustig.")]
    [InlineData("Ich weiß", "- Nein.")]
    [InlineData("♪ La la la ♪", "Was war das")]
    public void Detect_SentenceEndingOrDialogOrMusic_NotMerged(string text1, string text2)
    {
        var subtitles = new List<SubtitleLineViewModel>
        {
            MakeSubtitle(text1, 1.00, 2.00),
            MakeSubtitle(text2, 2.10, 3.00),
        };

        var candidates = MergeContinuationLinesHelper.Detect(subtitles, "de", maxGapMs: 500, maxTotalLength: 200);

        Assert.Empty(candidates);
    }

    [Fact]
    public void Apply_AllCandidates_SameResultAsSharedSubtitleMerge()
    {
        // seconv runs MergeContinuationLinesUtils on a Subtitle; it must merge exactly what the
        // tool merges with every candidate checked.
        var lines = new (string Text, int StartMs, int EndMs)[]
        {
            ("an", 1000, 1100),
            ("illusion,", 1120, 1300),
            ("an echo.", 1320, 1600),
            ("- Really?", 2000, 2500),
            ("- Yes.", 2600, 3000),
            ("But then I", 3100, 3500),
            ("forgot what it was.", 4500, 5000),
            ("<i>Das ist die Straße</i>", 5100, 5500),
            ("<i>nach Hause.</i>", 5600, 6000),
        };
        var viewModels = lines.Select(l => new SubtitleLineViewModel
        {
            Text = l.Text,
            StartTime = TimeSpan.FromMilliseconds(l.StartMs),
            EndTime = TimeSpan.FromMilliseconds(l.EndMs),
        }).ToList();
        var subtitle = new Subtitle();
        foreach (var l in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(l.Text, l.StartMs, l.EndMs));
        }

        var candidates = MergeContinuationLinesHelper.Detect(viewModels, "en", maxGapMs: 500, maxTotalLength: 200);
        var expected = MergeContinuationLinesHelper.Apply(viewModels, candidates, "en");
        var actual = MergeContinuationLinesUtils.MergeContinuationLinesInSubtitle(subtitle, "en", 500, 200);

        Assert.Equal(
            expected.Select(l => (l.Text, l.StartTime.TotalMilliseconds, l.EndTime.TotalMilliseconds)),
            actual.Paragraphs.Select(p => (p.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)));
    }
}
