using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class SmpteTimingDetectorTests
{
    private static string Tt(string attributes) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" " + attributes + ">\n" +
        "<body><div><p begin=\"00:00:01:00\" end=\"00:00:02:00\">Hi</p></div></body></tt>";

    [Theory]
    [InlineData("ttp:timeBase=\"smpte\" ttp:frameRate=\"24\" ttp:frameRateMultiplier=\"1000 1001\" ttp:dropMode=\"nonDrop\"")]
    [InlineData("ttp:timeBase=\"smpte\" ttp:frameRate=\"30\" ttp:frameRateMultiplier=\"1000 1001\"")]
    [InlineData("ttp:timeBase=\"smpte\" ttp:frameRate=\"24\" ttp:frameRateMultiplier=\"999 1000\" ttp:dropMode=\"nonDrop\"")]
    [InlineData("ttp:timeBase=\"smpte\" ttp:frameRate=\"60\" ttp:frameRateMultiplier=\"1000  1001\"")]
    public void NonDropSmpteAtNtscRate_IsDetected(string attributes)
    {
        Assert.True(SmpteTimingDetector.IsSmpteTimedText(Tt(attributes)));
    }

    [Theory]
    [InlineData("ttp:timeBase=\"media\" ttp:frameRate=\"24\" ttp:frameRateMultiplier=\"1000 1001\"")] // media time is already real time
    [InlineData("ttp:timeBase=\"smpte\" ttp:frameRate=\"30\" ttp:frameRateMultiplier=\"1000 1001\" ttp:dropMode=\"dropNTSC\"")] // drop-frame tracks the clock
    [InlineData("ttp:timeBase=\"smpte\" ttp:frameRate=\"25\" ttp:frameRateMultiplier=\"1 1\"")] // PAL, no pull-down
    [InlineData("ttp:timeBase=\"smpte\" ttp:frameRate=\"24\"")] // true 24 fps
    [InlineData("ttp:frameRate=\"24\" ttp:frameRateMultiplier=\"1000 1001\"")] // no time base = media
    public void OtherTimedText_IsNotDetected(string attributes)
    {
        Assert.False(SmpteTimingDetector.IsSmpteTimedText(Tt(attributes)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("[Script Info]\ntimeBase=smpte")]
    [InlineData("<tt ttp:timeBase=\"smpte\"")]
    public void NotTimedText_IsNotDetected(string? header)
    {
        Assert.False(SmpteTimingDetector.IsSmpteTimedText(header));
    }

    [Fact]
    public void LoadedTimedTextHeader_IsDetected()
    {
        var text = Tt("ttp:timeBase=\"smpte\" ttp:frameRate=\"24\" ttp:frameRateMultiplier=\"1000 1001\" ttp:dropMode=\"nonDrop\"");
        var lines = text.Split('\n').ToList();
        var subtitle = new Subtitle();
        var format = new TimedText10();
        var frameRate = Configuration.Settings.General.CurrentFrameRate; // the reader sets it from ttp:frameRate
        try
        {
            Assert.True(format.IsMine(lines, null));
            format.LoadSubtitle(subtitle, lines, null);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = frameRate;
        }

        Assert.Single(subtitle.Paragraphs);
        Assert.True(SmpteTimingDetector.IsSmpteTimedText(subtitle.Header));
    }
}
