using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Xunit;

namespace LibSETests.SubtitleFormats;

public class SoftNiSubTest
{
    // Regression for issue #10193: the *TIMING* line of exported SoftNi files
    // used to hardcode "1 25 0" regardless of the project frame rate, which
    // made players interpret the hh:mm:ss.ff time codes at the wrong speed for
    // non-25 fps projects. The second token must be the project frame rate
    // (rounded to the nearest integer, matching other SoftNi writers).
    [Theory]
    [InlineData(23.976, "1 24 0")]
    [InlineData(25.0, "1 25 0")]
    [InlineData(29.97, "1 30 0")]
    public void SoftNiSubToText_WritesProjectFrameRateInTimingLine(double frameRate, string expectedTimingLine)
    {
        var originalFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = frameRate;
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1000));

            var text = new SoftNiSub().ToText(subtitle, "Test");
            var lines = text.SplitToLines();
            var timingIndex = lines.FindIndex(line => line == "*TIMING*");
            Assert.True(timingIndex >= 0, "Missing *TIMING* section");
            Assert.True(timingIndex + 1 < lines.Count, "Missing timing line after *TIMING* section");
            Assert.Equal(expectedTimingLine, lines[timingIndex + 1]);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalFrameRate;
        }
    }

    [Fact]
    public void SoftNiColonSubToText_WritesProjectFrameRateInTimingLine()
    {
        var originalFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 23.976;
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1000));

            var text = new SoftNicolonSub().ToText(subtitle, "Test");
            var lines = text.SplitToLines();
            var timingIndex = lines.FindIndex(line => line == "*TIMING*");
            Assert.True(timingIndex >= 0, "Missing *TIMING* section");
            Assert.True(timingIndex + 1 < lines.Count, "Missing timing line after *TIMING* section");
            Assert.Equal("1 24 0", lines[timingIndex + 1]);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalFrameRate;
        }
    }
}
