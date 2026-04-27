using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

[Collection("NonParallelTests")]
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

    [Fact]
    public void ToText_Uses_Properties_From_Header()
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        var oldTimeCodeFormat = Configuration.Settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat;

        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            Configuration.Settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat = "hh:mm:ss.ms";

            var sut = new TimedTextImsc11();
            var subtitle = new Subtitle
            {
                Header = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                         "<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" " +
                         "xml:lang=\"de-DE\" ttp:profile=\"http://www.w3.org/ns/ttml/profile/imsc1/text\" ttp:timeBase=\"smpte\" ttp:frameRate=\"30\" ttp:frameRateMultiplier=\"1000 1001\" ttp:dropMode=\"dropNTSC\">" +
                         "<head>" +
                         "<metadata><ttm:title>Original title</ttm:title></metadata>" +
                         "<styling><style xml:id=\"style.center\" tts:color=\"#00ff00\"/></styling>" +
                         "<layout><region xml:id=\"region.bottomCenter\" tts:origin=\"10% 70%\" tts:extent=\"80% 20%\" tts:displayAlign=\"after\" tts:textAlign=\"center\"/></layout>" +
                         "</head>" +
                         "<body style=\"style.center\" region=\"region.bottomCenter\"><div /></body>" +
                         "</tt>",
            };
            subtitle.Paragraphs.Add(new Paragraph("Default region", 1000, 2000));
            subtitle.Paragraphs.Add(new Paragraph("{\\an8}Top region", 3000, 4000));

            var xml = sut.ToText(subtitle, "Fallback title");

            Assert.Contains("xml:lang=\"de-DE\"", xml);
            Assert.Contains("ttp:timeBase=\"smpte\"", xml);
            Assert.Contains("ttp:frameRate=\"30\"", xml);
            Assert.Contains("ttp:frameRateMultiplier=\"1000 1001\"", xml);
            Assert.Contains("ttp:dropMode=\"dropNTSC\"", xml);
            Assert.Contains("<ttm:title>Original title</ttm:title>", xml);
            Assert.Contains("xml:id=\"style.center\" tts:color=\"#00ff00\"", xml);
            Assert.Contains("xml:id=\"region.bottomCenter\" tts:origin=\"10% 70%\"", xml);
            Assert.DoesNotContain("xml:id=\"style.center\" tts:color=\"#ffffff\"", xml);
            Assert.DoesNotContain("xml:id=\"region.bottomCenter\" tts:origin=\"17.583% 73.414%\"", xml);
            Assert.Contains("<body style=\"style.center\" region=\"region.bottomCenter\">", xml);
            Assert.Contains("<p begin=\"00:00:01.000\" end=\"00:00:02.000\" region=\"region.bottomCenter\">", xml);
            Assert.Contains("<p begin=\"00:00:03.000\" end=\"00:00:04.000\" region=\"region.topCenter\">", xml);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            Configuration.Settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat = oldTimeCodeFormat;
        }
    }
}
