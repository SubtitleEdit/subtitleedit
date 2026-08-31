using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class NormalizeStringsTest
{
    private static string Fix(string input, string language = "en")
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        subtitle.Renumber();
        new NormalizeStrings().Fix(subtitle, new EmptyFixCallback { Language = language });
        return subtitle.Paragraphs[0].Text;
    }

    // Issue #14092: machine translation answers plain ASCII input with typographic punctuation.
    [Fact]
    public void ReplacesCurlyQuotesWithStraightOnes()
    {
        Assert.Equal("He said \"no way\" and left.", Fix("He said “no way” and left."));
        Assert.Equal("It's a so-called 'friend'.", Fix("It’s a so-called ‘friend’."));
    }

    [Fact]
    public void ReplacesTheOneCharacterEllipsisWithThreeDots()
    {
        Assert.Equal("Wait... what?", Fix("Wait… what?"));
    }

    [Fact]
    public void ReplacesPrimesWithQuotes()
    {
        Assert.Equal("6'2\" tall", Fix("6′2″ tall"));
    }

    [Fact]
    public void ReplacesDashesWithHyphens()
    {
        Assert.Equal("Books and comics - the whole lot.", Fix("Books and comics – the whole lot."));
        Assert.Equal("A non-breaking hyphen.", Fix("A non‑breaking hyphen."));
    }

    /// <summary>
    /// German and several of its neighbours quote with „…“ and ‚…‘ - those are the language's own
    /// quotation marks, not something to normalize away.
    /// </summary>
    [Fact]
    public void KeepsTheQuotationMarksOfLanguagesThatUseLowQuotes()
    {
        const string german = "Er sagte „Das geht nicht“ und ging.";
        Assert.Equal(german, Fix(german, "de"));

        const string polish = "To tak zwany ‚przyjaciel‘.";
        Assert.Equal(polish, Fix(polish, "pl"));
    }

    /// <summary>The rest of the rule still applies to those languages.</summary>
    [Fact]
    public void StillNormalizesDashesAndEllipsisForLowQuoteLanguages()
    {
        Assert.Equal("Bücher und Comics - alles.", Fix("Bücher und Comics – alles.", "de"));
        Assert.Equal("Warte... was?", Fix("Warte… was?", "de"));
    }

    /// <summary>
    /// Verbatim DeepL output for English input that had none of these characters: "It's a so-called
    /// 'friend' - nothing more." / "We took all our books and comics - the whole lot."
    /// </summary>
    [Fact]
    public void NormalizesRealDeepLOutput()
    {
        Assert.Equal(
            "Het is een zogenaamde 'vriend' - meer niet.",
            Fix("Het is een zogenaamde \u2018vriend\u2019 \u2013 meer niet.", "nl"));

        Assert.Equal(
            "We hebben al onze boeken en stripboeken meegenomen - alles.",
            Fix("We hebben al onze boeken en stripboeken meegenomen \u2013 alles.", "nl"));
    }

    [Fact]
    public void LeavesPlainTextAlone()
    {
        const string text = "Nothing to fix here - really.";
        Assert.Equal(text, Fix(text));
    }
}
