using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

namespace UITests.Features.Tools.SplitBreakLongLines;

public class SplitBreakLongLinesTests
{
    private static SubtitleLineViewModel MakeSubtitle(string text, double startSec, double endSec) =>
        new()
        {
            Text = text,
            StartTime = TimeSpan.FromSeconds(startSec),
            EndTime = TimeSpan.FromSeconds(endSec),
        };

    [Fact]
    public void Split_TextWithinLimit_ReturnsOriginalSubtitle()
    {
        var item = MakeSubtitle("This line is short enough.", 1, 3);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 80, singleLineMaxLength: 40);

        Assert.Single(result);
        Assert.Equal("This line is short enough.", result[0].Text);
    }

    [Fact]
    public void Split_LongText_DoesNotInsertLineBreaksIntoSegments()
    {
        // Regression for #10959: split-only should split into multiple subtitles
        // without adding a line break inside each new segment. AutoBreakLine
        // belongs to the opt-in Rebalance long lines step.
        var longText = new string('a', 45) + " " + new string('b', 45);
        var item = MakeSubtitle(longText, 1, 5);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: 50, singleLineMaxLength: 40);

        Assert.True(result.Count > 1, "Expected the long text to be split into multiple subtitles.");
        foreach (var line in result)
        {
            Assert.DoesNotContain("\n", line.Text);
            Assert.DoesNotContain("\r", line.Text);
        }
    }

    [Fact]
    public void Split_MaxNumberOfLinesOne_ProducesSingleLineSegments()
    {
        // Regression for #10959: when MaxNumberOfLines is 1
        // (maxCharactersPerSubtitle == singleLineMaxLength), each produced
        // subtitle must be a single short line with no embedded line break.
        const int singleLineMaxLength = 40;
        var longText = string.Join(' ', Enumerable.Repeat("word", 30));
        var item = MakeSubtitle(longText, 1, 10);

        var result = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle: singleLineMaxLength, singleLineMaxLength: singleLineMaxLength);

        Assert.True(result.Count > 1, "Expected the long text to be split into multiple subtitles.");
        foreach (var line in result)
        {
            Assert.DoesNotContain("\n", line.Text);
            Assert.DoesNotContain("\r", line.Text);
        }
    }
}
