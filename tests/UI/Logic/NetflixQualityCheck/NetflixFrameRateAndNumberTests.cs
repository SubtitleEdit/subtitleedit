using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

namespace UITests.Logic.NetflixQualityCheck;

/// <summary>
/// The Netflix checks take the video's frame rate from the controller, but measured times with
/// the app-wide CurrentFrameRate - so on any video whose rate differs from the setting the rules
/// were evaluated at one frame rate and fixed at another. Plus two defects in the
/// "spell out 1 to 10" rule.
/// </summary>
public class NetflixFrameRateAndNumberTests : IDisposable
{
    private readonly double _originalFrameRate = Configuration.Settings.General.CurrentFrameRate;

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _originalFrameRate;
        GC.SuppressFinalize(this);
    }

    private static Subtitle TwoLines(int firstEndMs, int secondStartMs)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("One", 0, firstEndMs));
        subtitle.Paragraphs.Add(new Paragraph("Two", secondStartMs, secondStartMs + 1000));
        return subtitle;
    }

    private static NetflixQualityController Controller(double videoFrameRate) =>
        new() { Language = "en", FrameRate = videoFrameRate };

    [Theory]
    [InlineData(25.0)]
    [InlineData(23.976)]
    public void TwoFramesGap_IsMeasuredAtTheVideoFrameRate(double appFrameRate)
    {
        // 61 ms is 1.46 frames at 23.976 - a violation of the two-frame rule whatever the app's
        // own frame rate happens to be set to. Measured at 25 it rounds to 2 frames and passed.
        Configuration.Settings.General.CurrentFrameRate = appFrameRate;
        var controller = Controller(23.976);

        new NetflixCheckTwoFramesGap("x").Check(TwoLines(1000, 1061), controller);

        Assert.Single(controller.Records);
    }

    [Theory]
    [InlineData(25.0)]
    [InlineData(23.976)]
    public void BridgeGaps_IsMeasuredAtTheVideoFrameRate(double appFrameRate)
    {
        // 470 ms is 11 frames at 23.976, inside the "more than 2, less than half a second" range
        // the rule bridges. Measured at 25 it rounds to 12 and fell outside the range.
        Configuration.Settings.General.CurrentFrameRate = appFrameRate;
        var controller = Controller(23.976);

        new NetflixCheckBridgeGaps("x").Check(TwoLines(1000, 1470), controller);

        Assert.Single(controller.Records);
    }

    private static string SpellOut(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 3000));
        var controller = new NetflixQualityController { Language = "en" };
        new NetflixCheckNumbersOneToTenSpellOut("x").Check(subtitle, controller);
        return controller.Records.Count > 0 ? controller.Records[0].FixedParagraph!.Text : text;
    }

    [Fact]
    public void SpellOut_ALaterThousandsSeparatorDoesNotBlockAnEarlierNumber()
    {
        // The thousands-separator test scanned the whole rest of the line for ",<digit>", so the
        // "4,000" later in the sentence vetoed writing out the "3,".
        Assert.Equal("I have three, and 4,000 dollars.", SpellOut("I have 3, and 4,000 dollars."));
    }

    [Fact]
    public void SpellOut_StillSkipsAThousandsSeparatedNumber()
    {
        Assert.Equal("1,000 people and three dogs.", SpellOut("1,000 people and 3 dogs."));
    }

    [Fact]
    public void SpellOut_LeavesANumberedHeadingAlone()
    {
        // The 1-9 pass treats a following period as "part of a heading or a decimal"; the 10 pass
        // only looked for a colon, so "10." was written out where "3." was not.
        Assert.Equal("Chapter 10. Introduction", SpellOut("Chapter 10. Introduction"));
        Assert.Equal("Chapter 3. Introduction", SpellOut("Chapter 3. Introduction"));
    }

    [Theory]
    [InlineData("There were 10.", "There were ten.")]
    [InlineData("There were 3.", "There were three.")]
    [InlineData("Only 10 left.", "Only ten left.")]
    [InlineData("He said 3, then left.", "He said three, then left.")]
    public void SpellOut_StillWritesOutTheOrdinaryCases(string input, string expected)
    {
        Assert.Equal(expected, SpellOut(input));
    }

    [Theory]
    [InlineData("It is 3:15 now.")]
    [InlineData("It is 10:15 now.")]
    public void SpellOut_LeavesATimeCodeAlone(string text)
    {
        Assert.Equal(text, SpellOut(text));
    }
}
