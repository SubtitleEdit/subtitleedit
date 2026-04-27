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
}
