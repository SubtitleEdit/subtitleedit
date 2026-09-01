using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Common;

/// <summary>
/// Guard tests for the 2026-08-27 bug hunt: off-by-one guards, conditions written the wrong way
/// round, and index arithmetic that mixed two different coordinate spaces.
/// </summary>
public class BugHunt13Test
{
    [Fact]
    public void MillisecondsToTime_NeverProducesNegativeComponents()
    {
        // Each component was rounded rather than truncated, so rounding one up left a negative
        // remainder that cascaded: 3500 ms came out as 0:0:4:-500.
        foreach (var ms in new double[] { 0, 999, 1500, 3500, 59500, 100000, 2400000, 3599999 })
        {
            var time = ToolBox.MillisecondsToTime(ms);

            Assert.All(time, component => Assert.True(component >= 0, $"{ms} ms gave {string.Join(':', time)}"));
            Assert.Equal(ms, (time[0] * 3600000) + (time[1] * 60000) + (time[2] * 1000) + time[3]);
        }
    }

    [Fact]
    public void IsFullLineTag_TruncatedClosingTag_DoesNotThrow()
    {
        // IndexOf('>') returning -1 plus 1 gave Substring a negative length.
        var exception = Record.Exception(() => ContinuationUtilities.IsFullLineTag(">> <i>hello world</i", 0));

        Assert.Null(exception);
    }

    [Fact]
    public void ToggleCasing_AssaLineBreakAfterATag_StaysInPlace()
    {
        // "\N" was recorded at its offset in the input while every other tag used the offset in
        // the tag-stripped text, so a preceding tag shifted the break (or dropped it at the end).
        var result = @"{\an8}One\Ntwo".ToggleCasing(new AdvancedSubStationAlpha());

        Assert.Equal(@"{\an8}ONE\NTWO", result);
    }

    [Fact]
    public void ToggleCasing_AssaLineBreakWithoutATag_StillWorks()
    {
        Assert.Equal(@"HELLO\NWORLD", @"Hello\NWorld".ToggleCasing(new AdvancedSubStationAlpha()));
    }

    [Fact]
    public void MoveWordUp_NestedTags_AreClosedInnermostFirst()
    {
        // The closers were emitted in stack order and then reversed again, producing the crossed
        // "<i><b>Hello</i></b>".
        var moveWord = new MoveWordUpDown("Line one.", "<i><b>Hello world</b></i>");

        moveWord.MoveWordUp();

        Assert.DoesNotContain("</i></b>", moveWord.S1, System.StringComparison.Ordinal);
        Assert.Contains("</b></i>", moveWord.S1, System.StringComparison.Ordinal);
    }
}
