using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class TimedTextImsc11Test
{
    [Fact]
    public void Italic_Trailing_Space_Preserved()
    {
        var sut = new TimedTextImsc11();

        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                  "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" ttp:profile=\"http://www.w3.org/ns/ttml/profile/imsc1/text\" ttp:timeBase=\"media\" xml:lang=\"en\">" +
                  "<head><styling>" +
                  "<style xml:id=\"italic\" tts:fontStyle=\"italic\"/>" +
                  "</styling></head>" +
                  "<body><div>" +
                  "<p begin=\"00:00:01.000\" end=\"00:00:03.000\"><span style=\"italic\">word </span><span>next</span></p>" +
                  "</div></body></tt>";

        var subtitle = new Subtitle();
        sut.LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("<i>word </i>next", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void Italic_Leading_Space_Preserved()
    {
        var sut = new TimedTextImsc11();

        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                  "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" ttp:profile=\"http://www.w3.org/ns/ttml/profile/imsc1/text\" ttp:timeBase=\"media\" xml:lang=\"en\">" +
                  "<head><styling>" +
                  "<style xml:id=\"italic\" tts:fontStyle=\"italic\"/>" +
                  "</styling></head>" +
                  "<body><div>" +
                  "<p begin=\"00:00:01.000\" end=\"00:00:03.000\"><span>word</span><span style=\"italic\"> next</span></p>" +
                  "</div></body></tt>";

        var subtitle = new Subtitle();
        sut.LoadSubtitle(subtitle, xml.SplitToLines(), "test.xml");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("word<i> next</i>", subtitle.Paragraphs[0].Text);
    }
}
