using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class DvdStudioProTest
{
    private static string EncodeSingleParagraph(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000));
        var raw = new DvdStudioPro().ToText(subtitle, "title");
        var line = raw.SplitToLines().Last(l => l.Contains('\t'));
        return line.Substring(line.LastIndexOf('\t') + 1);
    }

    // The allBoldItalic line separator used to be a copy-paste of the underline+bold+italic
    // one ("^U^B^I|^I^B^U"), injecting underline toggles that the reader does not parse -
    // they came back as literal "^U" text.
    [Fact]
    public void ToTextAllBoldItalicTwoLinesHasNoUnderlineCodes()
    {
        var encoded = EncodeSingleParagraph("<b><i>Hello" + Environment.NewLine + "World</i></b>");
        Assert.Equal("^B^IHello^B^I|^I^BWorld^I^B", encoded);
    }

    [Fact]
    public void ToTextAllItalicTwoLines()
    {
        var encoded = EncodeSingleParagraph("<i>Hello" + Environment.NewLine + "World</i>");
        Assert.Equal("^IHello^I|^IWorld^I", encoded);
    }

    [Fact]
    public void BoldItalicTwoLinesRoundTrip()
    {
        var text = "<b><i>Hello" + Environment.NewLine + "World</i></b>";
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000));
        var format = new DvdStudioPro();
        var raw = format.ToText(subtitle, "title");

        var roundTripped = new Subtitle();
        format.LoadSubtitle(roundTripped, raw.SplitToLines(), null);

        // The reader re-tags each line separately, so don't expect the exact input text -
        // but the stray "^U" of the old separator must not appear, the styling must
        // survive, and the words must be intact.
        Assert.Single(roundTripped.Paragraphs);
        var result = roundTripped.Paragraphs[0].Text;
        Assert.DoesNotContain("^", result);
        Assert.Equal("Hello" + Environment.NewLine + "World", HtmlUtil.RemoveHtmlTags(result));
        foreach (var line in result.SplitToLines())
        {
            Assert.Contains("<i>", line);
            Assert.Contains("<b>", line);
        }
    }
}
