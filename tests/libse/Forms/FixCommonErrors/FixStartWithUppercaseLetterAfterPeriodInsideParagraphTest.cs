using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest
{
    private static string Fix(string input, string language)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        subtitle.Renumber();
        new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(subtitle, new EmptyFixCallback { Language = language });
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
}
