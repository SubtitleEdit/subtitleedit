using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;
using Xunit;

namespace UITests.Logic.NetflixQualityCheck;

public class NetflixCheckNumbersOneToTenSpellOutTests
{
    private static string RunFix(string text, string language)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 3000));
        var controller = new NetflixQualityController { Language = language };

        new NetflixCheckNumbersOneToTenSpellOut("test").Check(subtitle, controller);

        return controller.Records.Count > 0 && controller.Records[0].FixedParagraph != null
            ? controller.Records[0].FixedParagraph!.Text
            : text;
    }

    [Theory]
    [InlineData("en", "There were 3 of them.", "There were three of them.")]
    [InlineData("es", "Había 3 perros.", "Había tres perros.")]
    [InlineData("de", "Es waren 3 Hunde.", "Es waren drei Hunde.")]
    public void SpellsOutSingleDigitsInTheSubtitleLanguage(string language, string input, string expected)
    {
        Assert.Equal(expected, RunFix(input, language));
    }

    // "10" used to be replaced with the English "ten" no matter the subtitle language.
    [Theory]
    [InlineData("en", "There were 10 of them.", "There were ten of them.")]
    [InlineData("es", "Había 10 perros.", "Había diez perros.")]
    [InlineData("it", "C'erano 10 cani.", "C'erano dieci cani.")]
    [InlineData("fr", "Il y avait 10 chiens.", "Il y avait dix chiens.")]
    public void SpellsOutTenInTheSubtitleLanguage(string language, string input, string expected)
    {
        Assert.Equal(expected, RunFix(input, language));
    }

    // No spell-out list for these languages, so the digits must be left alone rather
    // than replaced with English words.
    [Theory]
    [InlineData("sv", "Det var 10 stycken.")]
    [InlineData("nl", "Er waren 10 honden.")]
    public void LeavesNumbersAloneWithoutASpellOutList(string language, string input)
    {
        Assert.Equal(input, RunFix(input, language));
    }
}
