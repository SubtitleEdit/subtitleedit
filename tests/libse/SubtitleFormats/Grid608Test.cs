using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class Grid608Test
{
    private static string GridRow(string text, char font = 'R')
    {
        var colors = new char[32];
        for (var i = 0; i < 32; i++)
        {
            colors[i] = i < text.Length && text[i] != ' ' ? '0' : '9';
        }

        return text.PadRight(32) + new string(colors) + new string(font, 32);
    }

    [Fact]
    public void LoadSubtitleReadsTextRowsFromGrid()
    {
        var format = new Grid608();
        var subtitle = new Subtitle();
        var lines = new List<string> { "1", "00:00:01,886 --> 00:00:03,819" };
        for (var i = 0; i < 13; i++)
        {
            lines.Add(GridRow(string.Empty));
        }

        lines.Add(GridRow("  - Previously on The Tudors..."));
        lines.Add(GridRow("  - Your Holy Father offs you"));
        lines.Add(string.Empty);
        lines.Add("2");
        lines.Add("00:00:03,854 --> 00:00:05,454");
        for (var i = 0; i < 14; i++)
        {
            lines.Add(GridRow(string.Empty));
        }

        lines.Add(GridRow("Ah, yes sir.", 'I'));

        Assert.True(format.IsMine(lines, null));
        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("- Previously on The Tudors..." + Environment.NewLine + "- Your Holy Father offs you", subtitle.Paragraphs[0].Text);
        Assert.Equal(1886, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3819, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("<i>Ah, yes sir.</i>", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void IsMineRejectsPlainSubRip()
    {
        var format = new Grid608();
        var lines = new List<string>
        {
            "1",
            "00:00:01,886 --> 00:00:03,820",
            "Hello world",
        };

        Assert.False(format.IsMine(lines, null));
    }

    [Fact]
    public void RoundTripKeepsTextAndTimes()
    {
        var format = new Grid608();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello" + Environment.NewLine + "world", 1886, 3820));

        var text = format.ToText(subtitle, "title");
        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, new List<string>(text.SplitToLines()), null);

        Assert.Single(reloaded.Paragraphs);
        Assert.Equal("Hello" + Environment.NewLine + "world", reloaded.Paragraphs[0].Text);
        Assert.Equal(1886, reloaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3820, reloaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }
}
