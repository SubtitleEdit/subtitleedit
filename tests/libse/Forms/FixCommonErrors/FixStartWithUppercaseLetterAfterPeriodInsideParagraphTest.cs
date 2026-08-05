using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest
{
    private static string Fix(string input, string language, params string[] abbreviations)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        subtitle.Renumber();
        var callbacks = new EmptyFixCallback
        {
            Language = language,
            Abbreviations = new HashSet<string>(abbreviations, StringComparer.OrdinalIgnoreCase),
        };
        new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(subtitle, callbacks);
        return subtitle.Paragraphs[0].Text;
    }

    // Dutch contraction after a mid-paragraph period: keep "'s" lowercase, capitalize the next word.
    [Theory]
    [InlineData("Het regelt. 's morgens werkt hij.", "Het regelt. 's Morgens werkt hij.")]
    [InlineData("Het regelt. 'S morgens werkt hij.", "Het regelt. 's Morgens werkt hij.")] // also fixes wrongly capitalized contraction
    [InlineData("Het regelt. 't kind speelt.", "Het regelt. 't Kind speelt.")]
    public void Dutch_KeepsContractionLowercaseAndCapitalizesNextWord(string input, string expected)
    {
        Assert.Equal(expected, Fix(input, "nl"));
    }

    // Regular Dutch capitalization after a period must still happen.
    [Fact]
    public void Dutch_CapitalizesNormalWordAfterPeriod()
    {
        Assert.Equal("Het is laat. Morgen kom ik.", Fix("Het is laat. morgen kom ik.", "nl"));
    }

    // The contraction must not be touched after an ellipsis (we don't force casing after "...").
    [Fact]
    public void Dutch_LeavesContractionAfterEllipsis()
    {
        Assert.Equal("Het regelt... 's morgens werkt hij.", Fix("Het regelt... 's morgens werkt hij.", "nl"));
    }

    // An abbreviation period is not a sentence ending, so the next word stays as it is (#13082).
    [Theory]
    [InlineData("Ik sprak dhr. de vries gisteren.")]
    [InlineData("Volgens mevr. jansen is alles geregeld.")]
    [InlineData("Dat bevestigde dr. de boer vanochtend.")]
    [InlineData("Ook prof. van den berg was aanwezig.")]
    [InlineData("Ga naar nr. twaalf aan het einde.")]
    [InlineData("We wachten ca. vijf minuten.")]
    [InlineData("Neem kleding, eten, drinken, enz. mee.")]
    public void Dutch_KeepsLowercaseAfterAbbreviation(string input)
    {
        Assert.Equal(input, Fix(input, "nl", "dhr.", "mevr.", "dr.", "prof.", "nr.", "ca.", "enz."));
    }

    // Abbreviations with an inner period are recognized without being listed.
    [Theory]
    [InlineData("Er waren o.a. fietsen en brommers.")]
    [InlineData("Neem de trein i.p.v. de bus.")]
    [InlineData("Dat betekent, d.w.z. dat we vertrekken.")]
    public void Dutch_KeepsLowercaseAfterMultiDotAbbreviation(string input)
    {
        Assert.Equal(input, Fix(input, "nl"));
    }

    // The abbreviation list is matched case insensitively - subtitles use both "Dr." and "dr.".
    [Theory]
    [InlineData("I met dr. smith today.")]
    [InlineData("I met Dr. smith today.")]
    [InlineData("I met DR. smith today.")]
    public void AbbreviationMatchIsCaseInsensitive(string input)
    {
        Assert.Equal(input, Fix(input, "en", "Dr."));
    }

    // An inner hyphen is part of the word, so Russian/Ukrainian "д-р." and "г-жа." are found.
    [Theory]
    [InlineData("Я видел д-р. иванова вчера.")]
    [InlineData("Я видел г-жа. иванова вчера.")]
    public void HyphenatedAbbreviationIsRecognized(string input)
    {
        Assert.Equal(input, Fix(input, "ru", "д-р.", "г-жа."));
    }

    // A dialog dash must not become part of the word.
    [Fact]
    public void DialogDashIsNotPartOfTheWord()
    {
        Assert.Equal("Ja. - Nee. Tot morgen.", Fix("Ja. - Nee. tot morgen.", "nl", "dhr."));
    }

    // A period that is not an abbreviation must still be fixed.
    [Fact]
    public void CapitalizesAfterNonAbbreviation()
    {
        Assert.Equal("Ik ga weg. Tot morgen.", Fix("Ik ga weg. tot morgen.", "nl", "dhr.", "enz."));
    }
}
