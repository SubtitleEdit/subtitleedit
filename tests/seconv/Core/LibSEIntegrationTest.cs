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
}
