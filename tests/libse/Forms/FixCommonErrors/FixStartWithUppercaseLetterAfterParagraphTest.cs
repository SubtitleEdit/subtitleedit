using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixStartWithUppercaseLetterAfterParagraphTest
{
    /// <summary>
    /// Fixes <paramref name="text"/> with <paramref name="previous"/> as the preceding subtitle
    /// and returns the fixed text.
    /// </summary>
    private static string Fix(string previous, string text, string language, params string[] abbreviations)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(previous, 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph(text, 2100, 4000));
        subtitle.Renumber();
        var callbacks = new EmptyFixCallback
        {
            Language = language,
            Abbreviations = new HashSet<string>(abbreviations, StringComparer.OrdinalIgnoreCase),
        };
        new FixStartWithUppercaseLetterAfterParagraph().Fix(subtitle, callbacks);
        return subtitle.Paragraphs[1].Text;
    }

    // The previous subtitle ends in an abbreviation, so this one continues the same sentence (#13082).
    [Fact]
    public void KeepsLowercaseWhenPreviousParagraphEndsWithAbbreviation()
    {
        Assert.Equal("de vries gisteren.", Fix("Ik sprak met dhr.", "de vries gisteren.", "nl", "dhr."));
    }

    // Same, but the abbreviation ends line one of a two-line subtitle.
    [Fact]
    public void KeepsLowercaseWhenFirstLineEndsWithAbbreviation()
    {
        var text = "Ik sprak met dhr." + Environment.NewLine + "de vries gisteren.";
        Assert.Equal(text, Fix("Vorige regel.", text, "nl", "dhr."));
    }

    // Multi-dot abbreviations are recognized without being listed.
    [Fact]
    public void KeepsLowercaseAfterMultiDotAbbreviation()
    {
        Assert.Equal("he is asleep.", Fix("It is 5 a.m.", "he is asleep.", "en"));
    }

    // A line ending with a quotation mark is not a paragraph end - the quote is not
    // preceded by sentence-ending punctuation, so the next line continues the sentence (#12227).
    [Fact]
    public void KeepsLowercaseWhenPreviousParagraphEndsWithQuote()
    {
        Assert.Equal("me ontmoeten.", Fix("\"u\"", "me ontmoeten.", "nl"));
    }

    // The previous subtitle is a one-letter fragment ("u") - not a paragraph end, so the
    // next line must not be capitalized (#12227).
    [Fact]
    public void KeepsLowercaseWhenPreviousParagraphIsSingleLetter()
    {
        Assert.Equal("me ontmoeten.", Fix("u", "me ontmoeten.", "nl"));
    }

    // Same, but the one-letter fragment is line one of a two-line subtitle.
    [Fact]
    public void KeepsLowercaseWhenFirstLineIsSingleLetter()
    {
        var text = "u" + Environment.NewLine + "me ontmoeten.";
        Assert.Equal(text, Fix("- Wilt", text, "nl"));
    }

    // A real sentence ending must still be fixed.
    [Fact]
    public void CapitalizesAfterSentenceEnding()
    {
        Assert.Equal("And this starts.", Fix("This ends here.", "and this starts.", "en", "dr."));
    }

    // ...also on line two of a two-line subtitle.
    [Fact]
    public void CapitalizesSecondLineAfterSentenceEnding()
    {
        var expected = "Dit is een zin." + Environment.NewLine + "En dit ook.";
        var input = "Dit is een zin." + Environment.NewLine + "en dit ook.";
        Assert.Equal(expected, Fix("Vorige regel.", input, "nl", "dhr."));
    }
}
