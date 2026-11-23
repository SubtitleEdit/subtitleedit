using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

namespace UITests.Features.Tools.SplitSubtitle;

public class SubtitleSplitterTests
{
    private static Subtitle MakeSubtitle(int paragraphCount, int textLength = 10)
    {
        var subtitle = new Subtitle();
        var text = new string('a', textLength);
        for (var i = 0; i < paragraphCount; i++)
        {
            var startMs = i * 1000;
            subtitle.Paragraphs.Add(new Paragraph(text, startMs, startMs + 900));
        }

        return subtitle;
    }

    [Theory]
    [InlineData(SubtitleSplitter.SplitMode.Lines)]
    [InlineData(SubtitleSplitter.SplitMode.Characters)]
    [InlineData(SubtitleSplitter.SplitMode.Time)]
    public void Split_Into6Parts_ProducesExactly6Parts(SubtitleSplitter.SplitMode mode)
    {
        // Regression test for the reported bug: "split into 6 parts shows 6 but only 5 files generated".
        var subtitle = MakeSubtitle(60);

        var parts = SubtitleSplitter.Split(subtitle, 6, mode);

        Assert.Equal(6, parts.Count);
    }

    [Theory]
    [InlineData(SubtitleSplitter.SplitMode.Lines)]
    [InlineData(SubtitleSplitter.SplitMode.Characters)]
    [InlineData(SubtitleSplitter.SplitMode.Time)]
    public void Split_KeepsEveryParagraph_AndDropsNone(SubtitleSplitter.SplitMode mode)
    {
        var subtitle = MakeSubtitle(60);

        var parts = SubtitleSplitter.Split(subtitle, 6, mode);

        // No paragraph may be lost or duplicated across the parts.
        Assert.Equal(subtitle.Paragraphs.Count, parts.Sum(p => p.Paragraphs.Count));
    }

    [Theory]
    [InlineData(2, SubtitleSplitter.SplitMode.Lines)]
    [InlineData(3, SubtitleSplitter.SplitMode.Characters)]
    [InlineData(5, SubtitleSplitter.SplitMode.Time)]
    public void Split_IntoN_ProducesExactlyNParts(int parts, SubtitleSplitter.SplitMode mode)
    {
        var subtitle = MakeSubtitle(50);

        var result = SubtitleSplitter.Split(subtitle, parts, mode);

        Assert.Equal(parts, result.Count);
    }

    [Fact]
    public void SplitByLines_DistributesEvenly_RemainderGoesToLastPart()
    {
        var subtitle = MakeSubtitle(11);

        var parts = SubtitleSplitter.Split(subtitle, 3, SubtitleSplitter.SplitMode.Lines);

        Assert.Equal(3, parts.Count);
        Assert.Equal(3, parts[0].Paragraphs.Count);
        Assert.Equal(3, parts[1].Paragraphs.Count);
        Assert.Equal(5, parts[2].Paragraphs.Count); // 11 - 2*3 = 5
    }

    [Fact]
    public void Split_ClampsPartsToParagraphCount()
    {
        var subtitle = MakeSubtitle(3);

        var parts = SubtitleSplitter.Split(subtitle, 10, SubtitleSplitter.SplitMode.Lines);

        Assert.Equal(3, parts.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Split_WithOnePartOrFewer_ReturnsWholeSubtitleAsSinglePart(int requested)
    {
        var subtitle = MakeSubtitle(20);

        var parts = SubtitleSplitter.Split(subtitle, requested, SubtitleSplitter.SplitMode.Lines);

        Assert.Single(parts);
        Assert.Equal(20, parts[0].Paragraphs.Count);
    }

    [Fact]
    public void Split_EmptySubtitle_ReturnsNoParts()
    {
        var parts = SubtitleSplitter.Split(new Subtitle(), 6, SubtitleSplitter.SplitMode.Lines);

        Assert.Empty(parts);
    }

    [Fact]
    public void SplitByCharacters_ShortTexts_StillReachesRequestedPartCount()
    {
        // Four "Hi" cues = 8 characters, limit 4: the +4 slack meant no cue ever crossed it and
        // one part came back instead of two.
        var subtitle = new Subtitle();
        for (var i = 0; i < 4; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph("Hi", i * 1000, i * 1000 + 900));
        }

        var parts = SubtitleSplitter.Split(subtitle, 2, SubtitleSplitter.SplitMode.Characters);

        Assert.Equal(2, parts.Count);
        Assert.Equal(4, parts.Sum(p => p.Paragraphs.Count));
    }

    [Fact]
    public void SplitByTime_CuesBunchedAtStart_StillReachesRequestedPartCount()
    {
        // All cues start in the first 3 s but the last one runs to 40 s: no start passed the 20 s
        // limit, so one part came back instead of two.
        var subtitle = new Subtitle();
        for (var i = 0; i < 4; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph("Line " + i, i * 1000, i == 3 ? 40_000 : i * 1000 + 900));
        }

        var parts = SubtitleSplitter.Split(subtitle, 2, SubtitleSplitter.SplitMode.Time);

        Assert.Equal(2, parts.Count);
        Assert.Equal(4, parts.Sum(p => p.Paragraphs.Count));
    }

    [Fact]
    public void SplitByTime_AfterLongGap_DoesNotGiveEachCueItsOwnPart()
    {
        // 10 cues at 0-9 s, then 10 cues at 100-109 s, 4 parts of ~27.5 s. The first cue after the
        // gap used to move the limit only one step, so the next cues each opened a one-cue part.
        var subtitle = new Subtitle();
        for (var i = 0; i < 10; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph("A" + i, i * 1000, i * 1000 + 900));
        }

        for (var i = 0; i < 10; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph("B" + i, 100_000 + i * 1000, 100_000 + i * 1000 + 900));
        }

        var parts = SubtitleSplitter.Split(subtitle, 4, SubtitleSplitter.SplitMode.Time);

        Assert.Equal(4, parts.Count);
        Assert.Equal(20, parts.Sum(p => p.Paragraphs.Count));
        Assert.Equal(10, parts[0].Paragraphs.Count);
        Assert.True(parts[1].Paragraphs.Count > 1);
    }

    [Theory]
    [InlineData(SubtitleSplitter.SplitMode.Lines)]
    [InlineData(SubtitleSplitter.SplitMode.Characters)]
    [InlineData(SubtitleSplitter.SplitMode.Time)]
    public void Split_AsManyPartsAsParagraphs_GivesOneParagraphPerPart(SubtitleSplitter.SplitMode mode)
    {
        var subtitle = MakeSubtitle(7);

        var parts = SubtitleSplitter.Split(subtitle, 7, mode);

        Assert.Equal(7, parts.Count);
        Assert.All(parts, p => Assert.Single(p.Paragraphs));
    }
}
