using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// "Remove all formatting" keeps a \N between text as a line break, but \N used as padding
// (e.g. repeated to lift a subtitle) must not leave empty lines behind (issue #15531).
public class FormattingRemoverTests
{
    [Theory]
    [InlineData("Hi\\N\\N\\N", "Hi")]
    [InlineData("\\N\\NHi", "Hi")]
    [InlineData("{\\fs20}Hi{\\fs1}\\N\\N", "Hi")]
    [InlineData("<i>Hi</i>", "Hi")]
    [InlineData("Hi\\h\\N\\h", "Hi ")]
    public void RemovesTagsAndPadding(string text, string expected)
    {
        Assert.Equal(expected, FormattingRemover.RemoveAll(text));
    }

    [Theory]
    [InlineData("A\\NB")]
    [InlineData("<i>A</i>\nB")]
    public void KeepsOneLineBreakBetweenText(string text)
    {
        Assert.Equal("A" + Environment.NewLine + "B", FormattingRemover.RemoveAll(text));
    }

    // An empty line between text may be an intended gap (e.g. between two dialogue lines), and
    // text without any formatting must come back unchanged.
    [Theory]
    [InlineData("- Hi\\N\\N- Bye", "- Hi\n\n- Bye")]
    [InlineData("A\\N\\N\\NB", "A\n\n\nB")]
    [InlineData("\\N\\NA\\N\\NB\\N", "A\n\nB")]
    public void KeepsEmptyLinesBetweenText(string text, string expected)
    {
        Assert.Equal(expected.Replace("\n", Environment.NewLine), FormattingRemover.RemoveAll(text));
    }

    [Fact]
    public void UnformattedTextWithGapIsUnchanged()
    {
        var text = "A" + Environment.NewLine + Environment.NewLine + "B";
        Assert.Same(text, FormattingRemover.RemoveAll(text));
    }
}
