using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class ItunesTimedTextTest
{
    private static string RoundTrip(string input, out string raw)
    {
        var format = new ItunesTimedText();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));
        raw = sub.ToText(format);
        sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);
        Assert.Single(sub.Paragraphs);
        return sub.Paragraphs[0].Text;
    }

    [Fact]
    public void ItalicWithColorIsNested()
    {
        var input = "<i><font color=\"#EBEBEB\">Gut, jetzt hab ich's. Bereit?</font></i>" + Environment.NewLine +
                    "<i>- Moment ...</i>";

        var text = RoundTrip(input, out var raw);

        Assert.DoesNotContain("<span tts:fontStyle=\"italic\" />", raw);
        Assert.Contains("<span tts:fontStyle=\"italic\"><span tts:color=\"#EBEBEB\">Gut, jetzt hab ich's. Bereit?</span></span><br/><span tts:fontStyle=\"italic\">- Moment ...</span>", raw);
        Assert.Equal(input, text);
    }

    [Fact]
    public void ItalicOverTwoLinesIsReopened()
    {
        var input = "<i>Line one" + Environment.NewLine + "line two</i>";

        var text = RoundTrip(input, out var raw);

        Assert.Contains("<span tts:fontStyle=\"italic\">Line one</span><br/><span tts:fontStyle=\"italic\">line two</span>", raw);
        Assert.Equal("<i>Line one</i>" + Environment.NewLine + "<i>line two</i>", text);
    }

    [Fact]
    public void ColorAroundItalic()
    {
        var input = "<font color=\"red\">Red <i>italic</i> text</font>";

        var text = RoundTrip(input, out var raw);

        Assert.Contains("<span tts:color=\"red\">Red <span tts:fontStyle=\"italic\">italic</span> text</span>", raw);
        Assert.Equal(input, text);
    }

    [Fact]
    public void FontWithoutColorDoesNotCloseItalic()
    {
        var text = RoundTrip("<i>A <font face=\"Arial\">b</font> c</i>", out var raw);

        Assert.Contains("<span tts:fontStyle=\"italic\">A b c</span>", raw);
        Assert.Equal("<i>A b c</i>", text);
    }
}
