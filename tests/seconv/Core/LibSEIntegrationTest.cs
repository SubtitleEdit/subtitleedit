using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class LibSEIntegrationTest
{
    private static Subtitle MakeSubtitle(int count)
    {
        var sub = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            sub.Paragraphs.Add(new Paragraph($"line {i + 1}", i * 1000, i * 1000 + 500));
        }
        sub.Renumber();
        return sub;
    }

    [Fact]
    public void DeleteLast_CountEqualsTotal_RemovesAll()
    {
        var sub = MakeSubtitle(5);
        LibSEIntegration.DeleteLast(sub, 5);
        Assert.Empty(sub.Paragraphs);
    }

    [Fact]
    public void DeleteLast_CountGreaterThanTotal_RemovesAll()
    {
        var sub = MakeSubtitle(3);
        LibSEIntegration.DeleteLast(sub, 100);
        Assert.Empty(sub.Paragraphs);
    }

    [Fact]
    public void DeleteLast_CountLessThanTotal_KeepsRemainder()
    {
        var sub = MakeSubtitle(5);
        LibSEIntegration.DeleteLast(sub, 2);
        Assert.Equal(3, sub.Paragraphs.Count);
        Assert.Equal("line 1", sub.Paragraphs[0].Text);
        Assert.Equal("line 3", sub.Paragraphs[2].Text);
    }

    [Fact]
    public void DeleteLast_ZeroCount_NoOp()
    {
        var sub = MakeSubtitle(3);
        LibSEIntegration.DeleteLast(sub, 0);
        Assert.Equal(3, sub.Paragraphs.Count);
    }

    [Theory]
    [InlineData(0)] // regression for #12437: zero must stay zero, not be floored to 1 ms
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(96)]
    public void BridgeGaps_LeavesGapFromMinimumMillisecondsBetweenLines(int minGap)
    {
        var saved = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
        try
        {
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = minGap;

            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("line 1", 1000, 2000));
            sub.Paragraphs.Add(new Paragraph("line 2", 2500, 3500)); // 500 ms gap
            sub.Renumber();

            LibSEIntegration.BridgeGaps(sub, 1000);

            var gap = sub.Paragraphs[1].StartTime.TotalMilliseconds - sub.Paragraphs[0].EndTime.TotalMilliseconds;
            Assert.Equal(minGap, gap);
        }
        finally
        {
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = saved;
        }
    }

    [Fact]
    public void BridgeGaps_ZeroMinGap_LeavesAlreadyTouchingLinesAlone()
    {
        var saved = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
        try
        {
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = 0;

            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("line 1", 1000, 2000));
            sub.Paragraphs.Add(new Paragraph("line 2", 2000, 3000)); // no gap
            sub.Renumber();

            LibSEIntegration.BridgeGaps(sub, 1000);

            Assert.Equal(2000, sub.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal(2000, sub.Paragraphs[1].StartTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = saved;
        }
    }

    private static Subtitle MakeContinuationSubtitle()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("I was going to", 1000, 2000));
        sub.Paragraphs.Add(new Paragraph("tell you something.", 2100, 3000)); // 100 ms gap
        sub.Paragraphs.Add(new Paragraph("But then I", 4000, 5000));
        sub.Paragraphs.Add(new Paragraph("forgot what it was.", 5300, 6000)); // 300 ms gap
        sub.Renumber();
        return sub;
    }

    [Fact]
    public void ApplyOperations_MergeContinuationLines_DefaultGapIs250AndRenumbers()
    {
        var sub = MakeContinuationSubtitle();

        LibSEIntegration.ApplyOperations(sub, new List<string> { "MergeContinuationLines" });

        Assert.Equal(3, sub.Paragraphs.Count);
        Assert.Equal("I was going to tell you something.", sub.Paragraphs[0].Text.Replace(Environment.NewLine, " "));
        Assert.Equal(1000, sub.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, sub.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("But then I", sub.Paragraphs[1].Text);
        Assert.Equal(new[] { 1, 2, 3 }, sub.Paragraphs.Select(p => p.Number));
    }

    [Fact]
    public void ApplyOperations_MergeContinuationLines_UsesGivenMaxGap()
    {
        var sub = MakeContinuationSubtitle();

        LibSEIntegration.ApplyOperations(sub, new List<string> { "MergeContinuationLines" }, mergeContinuationLinesMaxGapMs: 500);

        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal("But then I forgot what it was.", sub.Paragraphs[1].Text.Replace(Environment.NewLine, " "));
        Assert.Equal(6000, sub.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void ApplyOperations_MergeContinuationLines_UsesGivenMaxCharacters()
    {
        // "I was going to" + "tell you something." is 33 characters - not below 33.
        var sub = MakeContinuationSubtitle();

        LibSEIntegration.ApplyOperations(sub, new List<string> { "MergeContinuationLines" }, mergeContinuationLinesMaxCharacters: 33);

        Assert.Equal(4, sub.Paragraphs.Count);
    }

    [Fact]
    public void ApplyOperations_MergeContinuationLines_DefaultMaxCharactersIsLineLengthTimesLines()
    {
        var savedLength = Configuration.Settings.General.SubtitleLineMaximumLength;
        var savedLines = Configuration.Settings.General.MaxNumberOfLines;
        try
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 11;
            Configuration.Settings.General.MaxNumberOfLines = 3; // 33: the first pair is not merged

            var sub = MakeContinuationSubtitle();
            LibSEIntegration.ApplyOperations(sub, new List<string> { "MergeContinuationLines" });
            Assert.Equal(4, sub.Paragraphs.Count);

            Configuration.Settings.General.SubtitleLineMaximumLength = 17; // 51: merged

            sub = MakeContinuationSubtitle();
            LibSEIntegration.ApplyOperations(sub, new List<string> { "MergeContinuationLines" });
            Assert.Equal(3, sub.Paragraphs.Count);
        }
        finally
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = savedLength;
            Configuration.Settings.General.MaxNumberOfLines = savedLines;
        }
    }
}
