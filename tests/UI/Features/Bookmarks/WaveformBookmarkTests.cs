using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

namespace UITests.Features.Bookmarks;

public class WaveformBookmarkTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("  ", "")]
    [InlineData("Note", "Note")]
    [InlineData("\n  First line \r\nSecond", "First line")]
    public void GetLabel_IsFirstNonEmptyLine(string? bookmark, string expected)
    {
        Assert.Equal(expected, WaveformBookmark.GetLabel(bookmark));
    }

    [Fact]
    public void GetLabel_CutsLongNotes()
    {
        var label = WaveformBookmark.GetLabel(new string('x', 100));
        Assert.Equal(WaveformBookmark.MaxLabelLength, label.Length);
        Assert.EndsWith("…", label);
    }

    [Fact]
    public void UseDarkForeground_FollowsLuminance()
    {
        Assert.True(WaveformBookmark.UseDarkForeground(255, 220, 80));
        Assert.False(WaveformBookmark.UseDarkForeground(40, 60, 160));
    }
}
