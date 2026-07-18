using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixUppercaseIInsideWordsTest
{
    private static string Fix(string input)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        subtitle.Renumber();
        new FixUppercaseIInsideWords().Fix(subtitle, new EmptyFixCallback());
        return subtitle.Paragraphs[0].Text;
    }

    // "HIt" ending the (punctuation-stripped) text: the old "Length - 3" guard left the
    // after-char unset for the last possible match, skipping the consonant check, so the
    // I was turned into an l ("You Hlt.") instead of lowercased ("You Hit.").
    [Theory]
    [InlineData("You HIt.", "You Hit.")]
    [InlineData("You HIt me.", "You Hit me.")] // mid-text match - worked before, must keep working
    public void UppercaseIBeforeLowercaseConsonantIsLowercased(string input, string expected)
    {
        Assert.Equal(expected, Fix(input));
    }
}
