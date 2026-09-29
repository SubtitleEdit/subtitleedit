using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// IMSC image profile documents reference png files with smpte:backgroundImage on a div
/// (embedded &lt;smpte:image&gt; is prohibited there). The reader only knew &lt;image src&gt;.
/// </summary>
public class TimedTextImageTest
{
    // The W3C IMSC1 image profile example (w3c/imsc, imsc1-image-example.xml), frame time codes.
    private const string W3cExample = """
        <?xml version="1.0" encoding="UTF-8"?>
        <tt xml:lang="fr"
            xmlns="http://www.w3.org/ns/ttml"
            xmlns:ttm="http://www.w3.org/ns/ttml#metadata"
            xmlns:tts="http://www.w3.org/ns/ttml#styling"
            xmlns:ttp="http://www.w3.org/ns/ttml#parameter"
            xmlns:smpte="http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt"
            xmlns:itts="http://www.w3.org/ns/ttml/profile/imsc1#styling"
            tts:extent="640px 480px"
            ttp:frameRate="25"
            ttp:profile="http://www.w3.org/ns/ttml/profile/imsc1/image">
            <head>
                <layout>
                    <region xml:id="region1" tts:origin="120px 410px" tts:extent="240px 40px" tts:showBackground="whenActive"/>
                    <region xml:id="region2" tts:origin="120px 20px" tts:extent="240px 40px" tts:showBackground="whenActive"/>
                </layout>
            </head>
            <body>
                <div region="region1" begin="00:00:01:00" end="00:00:02:00" smpte:backgroundImage="1.png"/>
                <div region="region1" begin="00:00:03:20" end="00:00:04:12" smpte:backgroundImage="2.png"/>
                <div region="region2" itts:forcedDisplay="true" begin="00:00:03:20" end="00:00:04:12" smpte:backgroundImage="3.png"/>
            </body>
        </tt>
        """;

    [Fact]
    public void ReadsBackgroundImageReferences()
    {
        var lines = W3cExample.SplitToLines();
        var format = new TimedTextImage();
        Assert.True(format.IsMine(lines, "example.ttml"));

        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, "example.ttml");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("1.png", subtitle.Paragraphs[0].Text);
        Assert.Equal("3.png", subtitle.Paragraphs[2].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(2000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void ReadsCuesInsideAnUntimedWrapperDiv()
    {
        var xml = W3cExample
            .Replace("<body>", "<body>\n<div>")
            .Replace("</body>", "</div>\n</body>");
        var lines = xml.SplitToLines();

        var subtitle = new Subtitle();
        new TimedTextImage().LoadSubtitle(subtitle, lines, "example.ttml");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal("2.png", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void EmbeddedImageFragmentIsNotAFileName()
    {
        // "#img0" points at an embedded <smpte:image> - Timed Text Base64 Image's job.
        var xml = W3cExample.Replace("smpte:backgroundImage=\"1.png\"", "smpte:backgroundImage=\"#img0\"");
        var subtitle = new Subtitle();
        new TimedTextImage().LoadSubtitle(subtitle, xml.SplitToLines(), "example.ttml");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("2.png", subtitle.Paragraphs[0].Text);
    }
}
