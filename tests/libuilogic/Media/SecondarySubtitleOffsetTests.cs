using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace LibUiLogicTests.Media;

/// <summary>
/// The video offset on a second subtitle (#16434): a sibling file with the same 10-hour times
/// takes it, a file already timed from zero does not.
/// </summary>
public class SecondarySubtitleOffsetTests
{
    private const double TenHoursMs = 10 * 60 * 60 * 1000;
    private const double VideoMs = 45 * 60 * 1000;

    private static Subtitle Make(double firstStartMs, int count = 20)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var start = firstStartMs + i * 60_000;
            subtitle.Paragraphs.Add(new Paragraph("Line " + i, start, start + 2000));
        }

        return subtitle;
    }

    [Fact]
    public void FileTimedWithOffset_AppliesOffset()
    {
        Assert.True(SecondarySubtitleOffset.ShouldApply(Make(TenHoursMs + 5000), TenHoursMs, 0, VideoMs));
    }

    [Fact]
    public void FileTimedFromZero_KeepsTimes()
    {
        Assert.False(SecondarySubtitleOffset.ShouldApply(Make(5000), TenHoursMs, 0, VideoMs));
    }

    [Fact]
    public void FileTimedFromZero_NoWindow_KeepsTimes()
    {
        Assert.False(SecondarySubtitleOffset.ShouldApply(Make(5000), TenHoursMs, 0, double.MaxValue));
    }

    [Fact]
    public void NegativeOffset_FileTimedWithOffset_AppliesOffset()
    {
        // Offset -2 s: file times are 2 s earlier than the video, the first line at -2 s + 5 s.
        Assert.True(SecondarySubtitleOffset.ShouldApply(Make(-2000 + 5000), -2000, 0, VideoMs));
    }

    [Fact]
    public void FitsNeitherWay_AppliesOffset()
    {
        Assert.True(SecondarySubtitleOffset.ShouldApply(Make(5 * TenHoursMs), TenHoursMs, 0, VideoMs));
    }

    [Fact]
    public void NoOffset_NeverApplies()
    {
        Assert.False(SecondarySubtitleOffset.ShouldApply(Make(5000), 0, 0, VideoMs));
    }
}
