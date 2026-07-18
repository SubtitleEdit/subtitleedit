using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class MicroDVDTest
{
    [Fact]
    public void LoadSubtitleRepairsLineWithBothTimeCodesEmpty()
    {
        var format = new MicroDvd();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "{0}{25}First line",
            "{}{}Second line",
            "{50}{75}Third line",
        };

        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(0, format.ErrorCount);
        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("Second line", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void LoadSubtitleRepairsLineWithOneTimeCodeEmpty()
    {
        var format = new MicroDvd();
        var subtitle = new Subtitle();
        var lines = new List<string>
        {
            "{}{25}First line",
            "{50}{75}Second line",
        };

        format.LoadSubtitle(subtitle, lines, null);

        Assert.Equal(0, format.ErrorCount);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("First line", subtitle.Paragraphs[0].Text);
    }
}
