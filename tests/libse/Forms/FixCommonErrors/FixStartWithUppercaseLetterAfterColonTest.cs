using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixStartWithUppercaseLetterAfterColonTest
{
    private static string Fix(string input, string language)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        subtitle.Renumber();
        new FixStartWithUppercaseLetterAfterColon().Fix(subtitle, new EmptyFixCallback { Language = language });
        return subtitle.Paragraphs[0].Text;
    }

    // Dutch contraction after a colon: keep "'s" lowercase, capitalize the next word.
    [Theory]
    [InlineData("Hij zei: 's morgens.", "Hij zei: 's Morgens.")]
    [InlineData("Hij zei: 'S morgens.", "Hij zei: 's Morgens.")] // also fixes wrongly capitalized contraction
    public void Dutch_KeepsContractionLowercaseAndCapitalizesNextWord(string input, string expected)
    {
        Assert.Equal(expected, Fix(input, "nl"));
    }

    // Regular capitalization after a colon must still happen for Dutch and English.
    [Theory]
    [InlineData("Hij zei: hallo.", "Hij zei: Hallo.", "nl")]
    [InlineData("He said: hello.", "He said: Hello.", "en")]
    public void CapitalizesNormalWordAfterColon(string input, string expected, string language)
    {
        Assert.Equal(expected, Fix(input, language));
    }
}
