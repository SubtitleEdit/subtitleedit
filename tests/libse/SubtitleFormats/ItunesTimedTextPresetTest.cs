using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// iTunes Timed Text export preset (#16257): the built-in header follows the default color /
/// body settings, a template header replaces the source header, and a line only names its
/// region when it differs from the body's.
/// </summary>
[Collection("NonParallelTests")]
public class ItunesTimedTextPresetTest
{
    private const string Template = """
        <?xml version="1.0" encoding="UTF-8"?>
        <tt xmlns="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling" xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xml:lang="de" ttp:timeBase="smpte" ttp:frameRate="25" ttp:frameRateMultiplier="1 1" ttp:dropMode="nonDrop">
          <head>
            <styling>
              <style tts:fontSize="100%" tts:color="#EBEBEB" tts:fontStyle="normal" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="normal"/>
              <style tts:fontSize="100%" tts:color="#EBEBEB" tts:fontStyle="italic" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="italic"/>
            </styling>
            <layout>
              <region xml:id="top" tts:origin="0% 0%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="before"/>
              <region xml:id="bottom" tts:origin="0% 85%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="after"/>
            </layout>
          </head>
          <body style="normal" region="bottom">
            <div/>
          </body>
        </tt>
        """;

    private const string WhiteSourceHeader = """
        <?xml version="1.0" encoding="UTF-8"?>
        <tt xmlns="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling" xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xml:lang="en" ttp:timeBase="smpte" ttp:frameRate="24" ttp:frameRateMultiplier="999 1000" ttp:dropMode="nonDrop">
          <head>
            <styling>
              <style tts:fontSize="100%" tts:color="white" tts:fontStyle="normal" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="normal"/>
              <style tts:fontSize="100%" tts:color="white" tts:fontStyle="italic" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="italic"/>
            </styling>
            <layout>
              <region xml:id="top" tts:origin="0% 0%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="before"/>
              <region xml:id="bottom" tts:origin="0% 85%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="after"/>
            </layout>
          </head>
          <body style="normal" region="bottom">
            <div/>
          </body>
        </tt>
        """;

    private sealed class SettingsSnapshot : IDisposable
    {
        private readonly string _color;
        private readonly string _style;
        private readonly string _region;
        private readonly string _template;
        private readonly string _language;
        private readonly double _frameRate;

        public SettingsSnapshot()
        {
            var ss = Configuration.Settings.SubtitleSettings;
            _color = ss.TimedTextItunesDefaultColor;
            _style = ss.TimedTextItunesDefaultStyle;
            _region = ss.TimedTextItunesDefaultRegion;
            _template = ss.TimedTextItunesHeaderTemplate;
            _language = ss.TimedTextItunesLanguage;
            _frameRate = Configuration.Settings.General.CurrentFrameRate;

            ss.TimedTextItunesDefaultColor = "white";
            ss.TimedTextItunesDefaultStyle = string.Empty;
            ss.TimedTextItunesDefaultRegion = string.Empty;
            ss.TimedTextItunesHeaderTemplate = string.Empty;
            ss.TimedTextItunesLanguage = string.Empty;
            Configuration.Settings.General.CurrentFrameRate = 23.976;
        }

        public void Dispose()
        {
            var ss = Configuration.Settings.SubtitleSettings;
            ss.TimedTextItunesDefaultColor = _color;
            ss.TimedTextItunesDefaultStyle = _style;
            ss.TimedTextItunesDefaultRegion = _region;
            ss.TimedTextItunesHeaderTemplate = _template;
            ss.TimedTextItunesLanguage = _language;
            Configuration.Settings.General.CurrentFrameRate = _frameRate;
        }
    }

    private static Subtitle MakeSubtitle(string? header, params string[] lines)
    {
        var sub = new Subtitle { Header = header };
        var start = 0;
        foreach (var line in lines)
        {
            sub.Paragraphs.Add(new Paragraph(line, start, start + 2000));
            start += 3000;
        }

        return sub;
    }

    private static int CountParagraphRegions(string raw, string region)
    {
        return Regex.Matches(raw, "<p [^>]*region=\"" + region + "\"").Count;
    }

    [Fact]
    public void BuiltInHeaderUsesDefaultColorAndBodyDefaults()
    {
        using var _ = new SettingsSnapshot();
        var ss = Configuration.Settings.SubtitleSettings;
        ss.TimedTextItunesDefaultColor = "#EBEBEB";
        ss.TimedTextItunesDefaultStyle = "normal";
        ss.TimedTextItunesDefaultRegion = "bottom";

        var raw = MakeSubtitle(null, "One", "Two").ToText(new ItunesTimedText());

        Assert.Equal(3, Regex.Matches(raw, "tts:color=\"#EBEBEB\"").Count);
        Assert.DoesNotContain("tts:color=\"white\"", raw);
        Assert.Contains("<body style=\"normal\" region=\"bottom\">", raw);
        Assert.Equal(0, CountParagraphRegions(raw, "bottom"));
    }

    [Fact]
    public void BuiltInHeaderWithoutBodyRegionNamesTheRegionOnEveryLine()
    {
        using var _ = new SettingsSnapshot();

        var raw = MakeSubtitle(null, "One", "{\\an8}Two").ToText(new ItunesTimedText());

        Assert.Contains("tts:color=\"white\"", raw);
        Assert.Contains("<body>", raw);
        Assert.Equal(1, CountParagraphRegions(raw, "bottom"));
        Assert.Equal(1, CountParagraphRegions(raw, "top"));
        Assert.DoesNotContain("{\\an8}", raw);
    }

    [Fact]
    public void BuiltInHeaderIgnoresUnknownBodyDefaults()
    {
        using var _ = new SettingsSnapshot();
        var ss = Configuration.Settings.SubtitleSettings;
        ss.TimedTextItunesDefaultStyle = "fancy";
        ss.TimedTextItunesDefaultRegion = "middle";
        ss.TimedTextItunesDefaultColor = "<bad>";

        var raw = MakeSubtitle(null, "One").ToText(new ItunesTimedText());

        Assert.Contains("<body>", raw);
        Assert.Contains("tts:color=\"white\"", raw);
    }

    [Fact]
    public void SourceHeaderIsKeptWithoutTemplate()
    {
        using var _ = new SettingsSnapshot();
        Configuration.Settings.SubtitleSettings.TimedTextItunesDefaultColor = "#EBEBEB";

        var raw = MakeSubtitle(WhiteSourceHeader, "One", "Two").ToText(new ItunesTimedText());

        Assert.Contains("tts:color=\"white\"", raw);
        Assert.DoesNotContain("#EBEBEB", raw);
        Assert.Contains("xml:lang=\"en\"", raw);
    }

    [Fact]
    public void SourceHeaderTimingParametersFollowCurrentFrameRate()
    {
        using var _ = new SettingsSnapshot();
        Configuration.Settings.General.CurrentFrameRate = 25;

        var raw = MakeSubtitle(WhiteSourceHeader, "One").ToText(new ItunesTimedText());

        Assert.Contains("ttp:frameRate=\"25\"", raw);
        Assert.Contains("ttp:frameRateMultiplier=\"1 1\"", raw);
        Assert.Contains("ttp:dropMode=\"nonDrop\"", raw);
        Assert.DoesNotContain("999 1000", raw);

        Configuration.Settings.General.CurrentFrameRate = 29.97;
        raw = MakeSubtitle(WhiteSourceHeader, "One").ToText(new ItunesTimedText());

        Assert.Contains("ttp:frameRate=\"30\"", raw);
        Assert.Contains("ttp:frameRateMultiplier=\"999 1000\"", raw);
        Assert.Contains("ttp:dropMode=\"dropNTSC\"", raw);
    }

    [Fact]
    public void OnlyTopLinesNameTheirRegionWhenBodyIsBottom()
    {
        using var _ = new SettingsSnapshot();

        var raw = MakeSubtitle(WhiteSourceHeader, "One", "{\\an8}Two", "Three").ToText(new ItunesTimedText());

        Assert.Equal(1, CountParagraphRegions(raw, "top"));
        Assert.Equal(0, CountParagraphRegions(raw, "bottom"));
        Assert.Contains(">Two</p>", raw);
    }

    [Fact]
    public void TemplateHeaderReplacesSourceHeader()
    {
        using var _ = new SettingsSnapshot();
        Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate = Template;
        Configuration.Settings.General.CurrentFrameRate = 23.976;

        var sub = MakeSubtitle(WhiteSourceHeader, "One", "{\\an8}Two");
        var raw = sub.ToText(new ItunesTimedText());

        Assert.Equal(2, Regex.Matches(raw, "tts:color=\"#EBEBEB\"").Count);
        Assert.DoesNotContain("tts:color=\"white\"", raw);
        Assert.Contains("xml:lang=\"de\"", raw);
        Assert.Contains("<body style=\"normal\" region=\"bottom\">", raw);
        Assert.Equal(1, CountParagraphRegions(raw, "top"));
        Assert.Equal(0, CountParagraphRegions(raw, "bottom"));

        // timing parameters follow the current frame rate, not the template's 25 fps
        Assert.Contains("ttp:frameRate=\"24\"", raw);
        Assert.Contains("ttp:frameRateMultiplier=\"999 1000\"", raw);
        Assert.Contains("begin=\"00:00:00:00\" end=\"00:00:02:00\"", raw);

        // the source header is left alone
        Assert.Contains("tts:color=\"white\"", sub.Header);
    }

    [Fact]
    public void TemplateHeaderAlsoReplacesTheBuiltInHeaderAndFollowsLanguageSetting()
    {
        using var _ = new SettingsSnapshot();
        Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate = Template;
        Configuration.Settings.SubtitleSettings.TimedTextItunesLanguage = "fr";

        var raw = MakeSubtitle(null, "Bonjour").ToText(new ItunesTimedText());

        Assert.Contains("tts:color=\"#EBEBEB\"", raw);
        Assert.Contains("xml:lang=\"fr\"", raw);
        Assert.DoesNotContain("xml:lang=\"de\"", raw);
    }

    [Fact]
    public void TemplateBodyDefaultsCanBeOverriddenBySettings()
    {
        using var _ = new SettingsSnapshot();
        var ss = Configuration.Settings.SubtitleSettings;
        ss.TimedTextItunesHeaderTemplate = Template;
        ss.TimedTextItunesDefaultStyle = "italic";
        ss.TimedTextItunesDefaultRegion = "top";

        var raw = MakeSubtitle(null, "One", "{\\an8}Two").ToText(new ItunesTimedText());

        Assert.Contains("<body style=\"italic\" region=\"top\">", raw);
        Assert.Equal(1, CountParagraphRegions(raw, "bottom"));
        Assert.Equal(0, CountParagraphRegions(raw, "top"));
    }

    [Fact]
    public void TemplateKeepsKnownParagraphStylesAndDropsUnknownOnes()
    {
        using var _ = new SettingsSnapshot();
        Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate = Template;

        var sub = MakeSubtitle(WhiteSourceHeader, "One", "Two", "Three");
        sub.Paragraphs[0].Style = "italic";
        sub.Paragraphs[1].Style = "fancy";
        sub.Paragraphs[2].Style = "normal"; // the body default
        var raw = sub.ToText(new ItunesTimedText());

        Assert.Single(Regex.Matches(raw, "<p [^>]*style=\"italic\""));
        Assert.DoesNotContain("style=\"fancy\"", raw);
        Assert.DoesNotContain("<p style=\"normal\"", raw);
    }

    [Fact]
    public void TemplateRoundTripsThroughTheReader()
    {
        using var _ = new SettingsSnapshot();
        Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate = Template;

        var raw = MakeSubtitle(null, "<i>One</i>", "{\\an8}Two").ToText(new ItunesTimedText());
        var sub = new Subtitle();
        new ItunesTimedText().LoadSubtitle(sub, raw.SplitToLines(), "x.itt");

        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal("<i>One</i>", sub.Paragraphs[0].Text);
        Assert.Equal("{\\an8}Two", sub.Paragraphs[1].Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not xml")]
    [InlineData("<root><head/><body/></root>")]
    [InlineData("<tt xmlns=\"http://www.w3.org/ns/ttml\"><body/></tt>")]
    public void LoadHeaderTemplateRejectsWhatIsNotTtml(string text)
    {
        Assert.Null(ItunesTimedText.LoadHeaderTemplate(text));
    }

    [Fact]
    public void LoadHeaderTemplateAddsMissingBodyAndDiv()
    {
        var xml = ItunesTimedText.LoadHeaderTemplate("<tt xmlns=\"http://www.w3.org/ns/ttml\"><head/></tt>");

        Assert.NotNull(xml);
        Assert.Contains("<body><div /></body>", xml!.OuterXml);
    }
}
