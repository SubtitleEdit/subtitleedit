using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

/// <summary>
/// OK in ASSA draw replaces only the rows the dialog imported as drawings - writing the result
/// over the selection in order overwrote ordinary dialogue lines that were selected too.
/// </summary>
public class AssaDrawRowDetectionTests
{
    [Theory]
    [InlineData("{\\p1}m 0 0 l 100 0 100 100{\\p0}")]
    [InlineData("{\\an7\\pos(0,0)\\p1}m 0 0 l 100 0 100 100")]
    [InlineData("{\\iclip(m 0 0 l 10 0 10 10)}")]
    [InlineData("{\\iclip(m 0 0 l 10 0 10 10)}{\\p1}m 0 0 l 100 0{\\p0}")]
    public void DrawingRows_AreDetected(string text)
    {
        Assert.True(MainViewModel.IsAssaDrawingText(text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Hello world")]
    [InlineData("{\\pos(10,20)}Hello")]
    [InlineData("{\\iclip(m 0 0 l 10 0 10 10)}Masked dialogue")]
    public void TextRows_AreNotDrawings(string text)
    {
        Assert.False(MainViewModel.IsAssaDrawingText(text));
    }
}
