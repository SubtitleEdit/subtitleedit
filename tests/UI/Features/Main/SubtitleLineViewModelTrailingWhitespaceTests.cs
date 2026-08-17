using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

/// <summary>
/// The edit text box is bound raw to <see cref="SubtitleLineViewModel.Text"/>, so pressing
/// Enter at the end of a subtitle leaves a trailing empty line in the row (#13389). SE4
/// trimmed in the TextChanged handler; SE5 restores the invariant at the commit points
/// instead: ToParagraph/ToParagraphOriginal (saving, every tool dialog) and
/// TrimTrailingTextWhitespace (row losing selection).
/// </summary>
public class SubtitleLineViewModelTrailingWhitespaceTests
{
    private static SubtitleLineViewModel MakeLine(string text)
    {
        return new SubtitleLineViewModel(new Paragraph(text, 0, 1000), new SubRip());
    }

    [Fact]
    public void ToParagraph_TrimsTrailingWhitespace()
    {
        var line = MakeLine("Hello there." + Environment.NewLine);

        var p = line.ToParagraph();

        Assert.Equal("Hello there.", p.Text);
        // The row itself is untouched - the text box may still be editing it.
        Assert.Equal("Hello there." + Environment.NewLine, line.Text);
    }

    [Fact]
    public void ToParagraph_KeepsInteriorEmptyLine()
    {
        var text = "One." + Environment.NewLine + Environment.NewLine + "Two.";
        var line = MakeLine(text);

        Assert.Equal(text, line.ToParagraph().Text);
    }

    [Fact]
    public void ToParagraphOriginal_TrimsTrailingWhitespace()
    {
        var line = MakeLine("Hello there.");
        line.OriginalText = "Original text." + Environment.NewLine;

        Assert.Equal("Original text.", line.ToParagraphOriginal().Text);
    }

    [Fact]
    public void TrimTrailingTextWhitespace_TrimsBothTextFields()
    {
        var line = MakeLine("Hello there." + Environment.NewLine);
        line.OriginalText = "Original text. ";

        line.TrimTrailingTextWhitespace();

        Assert.Equal("Hello there.", line.Text);
        Assert.Equal("Original text.", line.OriginalText);
    }

    [Fact]
    public void TrimTrailingTextWhitespace_LeavesCleanTextAlone()
    {
        var line = MakeLine("Hello there.");
        line.OriginalText = "Original text.";

        line.TrimTrailingTextWhitespace();

        Assert.Equal("Hello there.", line.Text);
        Assert.Equal("Original text.", line.OriginalText);
    }
}
