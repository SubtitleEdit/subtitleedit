using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;

namespace LibSETests.Common;

/// <summary>
/// Guard tests for the 2026-08-28 bug hunt (sweep 20): the whole-millisecond frame-rate
/// conversion seconv and batch convert now share, and two places that read a line break with
/// Environment.NewLine after deciding the text had one with a line-ending-agnostic helper.
/// </summary>
public class BugHunt20Test
{
    [Fact]
    public void ChangeFrameRateWholeMilliseconds_KeepsEqualLengthCuesEqual()
    {
        // Scaling start and end independently rounds the two ends apart: at 25 -> 23.976 the
        // first 1000 ms cue came out 1043 ms and the second - same length, one second later -
        // came out 1042 ms.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("1", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("2", 1000, 2000));

        subtitle.ChangeFrameRateWholeMilliseconds(25.0, 23.976);

        Assert.Equal(
            subtitle.Paragraphs[0].DurationTotalMilliseconds,
            subtitle.Paragraphs[1].DurationTotalMilliseconds);
    }

    [Fact]
    public void ChangeFrameRateWholeMilliseconds_LandsOnWholeMilliseconds()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("1", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("2", 2000, 3000));

        subtitle.ChangeFrameRateWholeMilliseconds(25.0, 30.0);

        foreach (var p in subtitle.Paragraphs)
        {
            Assert.Equal(p.StartTime.TotalMilliseconds, Math.Round(p.StartTime.TotalMilliseconds));
            Assert.Equal(p.EndTime.TotalMilliseconds, Math.Round(p.EndTime.TotalMilliseconds));
        }
    }

    [Fact]
    public void ChangeFrameRateWholeMilliseconds_DoesNotManufactureAnOverlap()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("1", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("2", 1000, 2000));

        subtitle.ChangeFrameRateWholeMilliseconds(25.0, 23.976);

        Assert.True(
            subtitle.Paragraphs[0].EndTime.TotalMilliseconds <= subtitle.Paragraphs[1].StartTime.TotalMilliseconds,
            "the rounding turned two touching cues into an overlap");
    }

    [Fact]
    public void ChangeFrameRate_StillReturnsTheFractionalResult()
    {
        // The fractional overload is public API and has its own callers - the whole-millisecond
        // behaviour is a separate method, not a change to this one.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("1", 0, 1000));

        subtitle.ChangeFrameRate(25.0, 30.0);

        Assert.True(Math.Abs(subtitle.Paragraphs[0].EndTime.TotalMilliseconds - 833.33333333333) < 0.01);
    }

    [Theory]
    // GetNumberOfLines counts '\n', so these count as two lines whatever the platform's
    // Environment.NewLine is - and the branch then indexed the line break with it.
    [InlineData("<i>- You think they're gone?<i>\nThat can't be.</i>")]
    [InlineData("<i>- You think they're gone?<i>\r\nThat can't be.</i>")]
    [InlineData("<i>Foo</i>\n<i>Bar</i>")]
    [InlineData("</i>Foo</i>\n</i>Bar</i>")]
    public void FixInvalidItalicTags_BareLineFeed_DoesNotThrow(string text)
    {
        var exception = Record.Exception(() => HtmlUtil.FixInvalidItalicTags(text));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("MAN:\nHello there.\n- And hello to you.")]
    [InlineData("MAN: Hello there.\n- And hello to you.")]
    [InlineData("(NOISE)\nHello there.")]
    public void RemoveTextFromHearImpaired_BareLineFeed_DoesNotThrow(string text)
    {
        var settings = new RemoveTextForHISettings(new Subtitle());
        var remover = new RemoveTextForHI(settings);

        var exception = Record.Exception(() => remover.RemoveTextFromHearImpaired(text, "en"));

        Assert.Null(exception);
    }
}
