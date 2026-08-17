using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class TMPlayerTest
{
    [Fact]
    public void IsMineAcceptsBlankSeparatorLines()
    {
        // files saved with "\r\r\n" line endings get a blank line after every cue when split
        // (see SplitToLines/#8854) - the blanks must not count as parse errors
        var format = new TMPlayer();
        var lines = new List<string>();
        for (var i = 0; i < 10; i++)
        {
            lines.Add($"00:00:{i * 5 + 1:00}:Line number {i}");
            lines.Add(string.Empty);
        }

        Assert.True(format.IsMine(lines, null));

        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, null);
        Assert.Equal(10, subtitle.Paragraphs.Count);
        Assert.Equal("Line number 0", subtitle.Paragraphs[0].Text);
    }
}
