using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;

namespace UITests.Features.Assa;

/// <summary>
/// Issue #15817: "Set background" previewed the middle of the line even when the main player was
/// paused on another frame inside it.
/// </summary>
public class AssSetBackgroundPreviewSeekTests
{
    private static Paragraph MakeParagraph() => new("text", 10000, 14000);

    [Theory]
    [InlineData(10.5, 10.5)]
    [InlineData(10.0, 10.0)]
    [InlineData(14.0, 14.0)]
    [InlineData(9.99, 12.0)]
    [InlineData(14.01, 12.0)]
    public void UsesPlayerPositionOnlyInsideLine(double position, double expected)
    {
        Assert.Equal(expected, AssSetBackgroundViewModel.GetPreviewSeekSeconds(MakeParagraph(), position), 3);
    }

    [Fact]
    public void NoPlayerPositionFallsBackToMiddle()
    {
        Assert.Equal(12.0, AssSetBackgroundViewModel.GetPreviewSeekSeconds(MakeParagraph(), null), 3);
    }
}
