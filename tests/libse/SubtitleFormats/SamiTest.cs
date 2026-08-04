using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class SamiTest
{
    private static Subtitle LoadSamiSubtitle(string content)
    {
        var subtitle = new Subtitle();
        var format = new Sami();
        var lines = new List<string>(content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
        format.LoadSubtitle(subtitle, lines, null);
        return subtitle;
    }

    [Fact]
    public void LoadSubtitleDoesNotThrowWhenStyleEndTagOccursBeforeStyleStartTag()
    {
        // A stray "</style>" before the "<style>" block used to produce a negative
        // substring length (crashing IsMine/format detection with ArgumentOutOfRangeException)
        var content = "x</style>\r\n" +
                      "<style type=\"text/css\">\r\n" +
                      "<sync start=1000><p class=enuscc>Hello\r\n" +
                      "<sync start=3000><p class=enuscc>&nbsp;";

        var subtitle = LoadSamiSubtitle(content);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void IsMineDoesNotThrowWhenStyleEndTagOccursBeforeStyleStartTag()
    {
        var lines = new List<string>
        {
            "x</style>",
            "<style type=\"text/css\">",
            "<sync start=1000><p class=enuscc>Hello",
            "<sync start=3000><p class=enuscc>&nbsp;",
        };

        Assert.True(new Sami().IsMine(lines, "test.smi"));
    }

    [Fact]
    public void LoadSubtitleSupportsEncryptedSyncTagsInAnyCasing()
    {
        var content = "<SAMI>\r\n" +
                      "<BODY>\r\n" +
                      "<SYNC Encrypted=\"true\" Start=1000><P Class=ENUSCC>Hello world\r\n" +
                      "<SYNC Encrypted=\"true\" Start=3000><P Class=ENUSCC>&nbsp;\r\n" +
                      "</BODY>\r\n" +
                      "</SAMI>";

        var subtitle = LoadSamiSubtitle(content);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello world", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }
}
