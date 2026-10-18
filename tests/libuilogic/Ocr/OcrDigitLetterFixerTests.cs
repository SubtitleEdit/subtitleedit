using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

public class OcrDigitLetterFixerTests
{
    [Theory]
    [InlineData("It costs 5O dollars.", "It costs 50 dollars.")]
    [InlineData("1OO", "100")]
    [InlineData("at 1O:3O tonight", "at 10:30 tonight")]
    [InlineData("1,OOO men", "1,000 men")]
    [InlineData("<i>2OO miles</i>", "<i>200 miles</i>")]
    [InlineData("He said 0f course.", "He said of course.")]
    [InlineData("I g0t it, t0o.", "I got it, too.")]
    [InlineData("0kay.", "Okay.")]
    [InlineData("Fine. 0kay.", "Fine. Okay.")]
    [InlineData("- 0kay.", "- Okay.")]
    [InlineData("STAND D0WN", "STAND DOWN")]
    [InlineData("7o/o of childhood accidents", "7% of childhood accidents")]
    [InlineData("34o/o of accidents", "34% of accidents")]
    [InlineData("<i>♪ D00-wah-doo-wah</i>", "<i>♪ Doo-wah-doo-wah</i>")]
    public void Fixes(string input, string expected)
    {
        Assert.Equal(expected, OcrDigitLetterFixer.Fix(input));
    }

    [Theory]
    [InlineData("O.K.")]
    [InlineData("Oh, 007 is here.")]
    [InlineData("CO2 and MP3")]
    [InlineData("1080p and Win10")]
    [InlineData("The 5Os were great")]
    [InlineData("O, Romeo")]
    [InlineData("100 or 0")]
    [InlineData("and/or he/she")]
    [InlineData("З000 долларов.")] // Cyrillic "З" read for "3" - still a number
    public void LeavesAlone(string input)
    {
        Assert.Equal(input, OcrDigitLetterFixer.Fix(input));
    }
}
