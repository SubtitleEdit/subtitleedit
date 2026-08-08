using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;
using Xunit;

namespace UITests.Logic.NetflixQualityCheck;

/// <summary>
/// The per-language limits are transcribed from the Netflix Timed Text Style Guides at
/// https://partnerhelp.netflixstudios.com - these tests pin the ones that have drifted.
/// </summary>
public class NetflixQualityControllerTests
{
    // Netflix raised the Russian limit from 39 to 42 CPR in the December 2025 TTSG update.
    [Theory]
    [InlineData("ru", 42)]
    [InlineData("en", 42)]
    [InlineData("th", 35)]
    [InlineData("ko", 16)]
    [InlineData("zh", 16)]
    public void SingleLineMaxLengthMatchesStyleGuide(string language, int expected)
    {
        Assert.Equal(expected, new NetflixQualityController { Language = language }.SingleLineMaxLength);
    }

    // Indonesian is "id" in ISO 639-1; the old "in" case never matched, so Indonesian
    // silently fell through to the no-space default.
    [Theory]
    [InlineData("id", DialogType.DashBothLinesWithSpace)]
    [InlineData("ko", DialogType.DashBothLinesWithSpace)]
    [InlineData("th", DialogType.DashBothLinesWithSpace)]
    [InlineData("bg", DialogType.DashSecondLineWithSpace)]
    [InlineData("nl", DialogType.DashSecondLineWithoutSpace)]
    [InlineData("fi", DialogType.DashSecondLineWithoutSpace)]
    [InlineData("de", DialogType.DashBothLinesWithoutSpace)]
    [InlineData("en", DialogType.DashBothLinesWithoutSpace)]
    public void SpeakerStyleMatchesStyleGuide(string language, DialogType expected)
    {
        Assert.Equal(expected, new NetflixQualityController { Language = language }.SpeakerStyle);
    }

    // "Do not use italics" appears verbatim in these guides.
    [Theory]
    [InlineData("ar")]
    [InlineData("he")]
    [InlineData("hi")]
    [InlineData("ko")]
    [InlineData("th")]
    [InlineData("zh")]
    public void ItalicsAreNotAllowed(string language)
    {
        Assert.False(new NetflixQualityController { Language = language }.AllowItalics);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("ru")]
    [InlineData("ja")]
    [InlineData("tr")]
    public void ItalicsAreAllowed(string language)
    {
        Assert.True(new NetflixQualityController { Language = language }.AllowItalics);
    }

    [Theory]
    [InlineData("en", false, false, 20)]
    [InlineData("en", true, false, 17)]
    [InlineData("en", false, true, 20)]
    [InlineData("hi", false, false, 22)]
    [InlineData("hi", false, true, 25)]
    [InlineData("hi", true, true, 20)]
    [InlineData("ko", false, false, 12)]
    [InlineData("ko", false, true, 14)]
    [InlineData("th", false, false, 17)]
    [InlineData("th", false, true, 20)]
    public void ReadingSpeedMatchesStyleGuide(string language, bool isChildrenProgram, bool isSdh, int expected)
    {
        var controller = new NetflixQualityController
        {
            Language = language,
            IsChildrenProgram = isChildrenProgram,
            IsSDH = isSdh,
        };

        Assert.Equal(expected, controller.CharactersPerSecond);
    }
}
