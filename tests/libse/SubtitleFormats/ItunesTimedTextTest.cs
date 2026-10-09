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
    [Fact]
    public void UnderlineIsWrittenAsTextDecoration()
    {
        var input = "<u>under</u> <i>it</i>";

        var text = RoundTrip(input, out var raw);

        Assert.Contains("<span tts:textDecoration=\"underline\">under</span> <span tts:fontStyle=\"italic\">it</span>", raw);
        Assert.Equal(input, text);
    }

    private const string Head = """
        <?xml version="1.0" encoding="UTF-8"?>
        <tt xmlns="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling" xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xml:lang="de" ttp:timeBase="smpte" ttp:frameRate="25" ttp:frameRateMultiplier="1 1" ttp:dropMode="nonDrop">
        <head><styling>
        <style tts:fontSize="100%" tts:color="white" tts:fontStyle="normal" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="normal"/>
        <style tts:fontSize="100%" tts:color="white" tts:fontStyle="italic" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="italic"/>
        <style tts:color="#FFFFFF" tts:fontStyle="italic" xml:id="style.italic.ffffff"/>
        </styling>
        <layout><region xml:id="top" tts:origin="0% 0%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="before"/>
        <region xml:id="bottom" tts:origin="0% 85%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="after"/></layout></head>
        <body BODYSTYLE region="bottom"><div>
        """;

    private static string Read(string paragraph, string bodyStyle = "style=\"normal\"")
    {
        var raw = Head.Replace("BODYSTYLE", bodyStyle) + Environment.NewLine + paragraph + Environment.NewLine + "</div></body></tt>";
        var sub = new Subtitle();
        new ItunesTimedText().LoadSubtitle(sub, raw.SplitToLines(), "x.itt");
        Assert.Single(sub.Paragraphs);
        return sub.Paragraphs[0].Text;
    }

    [Theory]
    [InlineData("style=\"normal\"")]
    [InlineData("")]
    public void ReadSpanStyleDoesNotAddDefaultFontAndColor(string bodyStyle)
    {
        var text = Read("<p begin=\"00:01:10:06\" end=\"00:01:13:04\"><span style=\"italic\">Gut, jetzt hab ich's. Bereit?<br/>- Moment ...</span></p>", bodyStyle);

        Assert.Equal("<i>Gut, jetzt hab ich's. Bereit?" + Environment.NewLine + "- Moment ...</i>", text);
    }

    [Fact]
    public void ReadSpanStyleWithSameColorAsDefault()
    {
        var text = Read("<p begin=\"00:01:10:06\" end=\"00:01:13:04\"><span style=\"style.italic.ffffff\">Gut</span><br /><span style=\"style.italic.ffffff\">- Moment</span></p>");

        Assert.Equal("<i>Gut</i>" + Environment.NewLine + "<i>- Moment</i>", text);
    }

    [Fact]
    public void ReadParagraphItalic()
    {
        Assert.Equal("<i>Attribute</i>", Read("<p begin=\"00:00:01:00\" end=\"00:00:02:00\" tts:fontStyle=\"italic\">Attribute</p>"));
        Assert.Equal("<i>Style</i>", Read("<p begin=\"00:00:01:00\" end=\"00:00:02:00\" style=\"italic\">Style</p>"));
    }

    [Fact]
    public void ReadOldFlatSpansSkipsEmptySpan()
    {
        var text = Read("<p begin=\"00:01:10:06\" end=\"00:01:13:04\"><span tts:fontStyle=\"italic\" /><span tts:color=\"#EBEBEB\">Gut</span><br/><span tts:fontStyle=\"italic\">- Moment</span></p>");

        Assert.Equal("<font color=\"#EBEBEB\">Gut</font>" + Environment.NewLine + "<i>- Moment</i>", text);
    }

    [Fact]
    public void ReadKeepsSpaceBetweenSpans()
    {
        var text = Read("<p begin=\"00:00:01:00\" end=\"00:00:02:00\"><span tts:fontStyle=\"italic\">a</span> <span tts:fontWeight=\"bold\">b</span></p>");

        Assert.Equal("<i>a</i> <b>b</b>", text);
    }

    [Fact]
    public void ReadExplicitColorIsKept()
    {
        var text = Read("<p begin=\"00:00:01:00\" end=\"00:00:02:00\"><span tts:color=\"yellow\">a</span> <span style=\"italic\" tts:fontFamily=\"Arial\">b</span></p>");

        Assert.Equal("<font color=\"yellow\">a</font> <i><font face=\"Arial\">b</font></i>", text);
    }
}
