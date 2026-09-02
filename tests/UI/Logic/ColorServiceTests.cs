using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class ColorServiceTests
{
    private static readonly Color Red = Color.FromRgb(0xFF, 0x00, 0x00);
    private static readonly Color Blue = Color.FromRgb(0x00, 0x00, 0xFF);

    private static SubtitleLineViewModel Line(string text) => new() { Text = text };

    [Fact]
    public void SetColorTag_WebVtt_WrapsTextInClassTag()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTT();

        var result = service.SetColorTag("Hello", Red, subtitle, format);

        Assert.Contains("<c.", result);
        Assert.Contains("Hello", result);
        Assert.Contains("</c>", result);
        Assert.Contains("STYLE", subtitle.Header);
    }

    [Fact]
    public void SetColorTag_Assa_PrependsAssaColorTag()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new AdvancedSubStationAlpha();

        var result = service.SetColorTag("Hello", Red, subtitle, format);

        Assert.StartsWith("{\\", result);
        Assert.EndsWith("Hello", result);
    }

    [Fact]
    public void SetColorTag_Srt_WrapsInFontTag()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new SubRip();

        var result = service.SetColorTag("Hello", Red, subtitle, format);

        Assert.Equal("<font color=\"#FF0000\">Hello</font>", result);
    }

    [Fact]
    public void RemoveColorTag_WebVtt_StripsClassWrapperForGivenColor()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTT();

        var withColor = service.SetColorTag("Hello", Red, subtitle, format);
        var stripped = service.RemoveColorTag(withColor, Red, subtitle, format);

        Assert.DoesNotContain("<c.", stripped);
        Assert.DoesNotContain("</c>", stripped);
        Assert.Contains("Hello", stripped);
    }

    [Fact]
    public void RemoveColorTags_WebVtt_StripsAllColorWrappersFromAllLines()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTT();

        var lines = new List<SubtitleLineViewModel>
        {
            Line("Hello"),
            Line("World"),
        };

        service.SetColor(lines, Red, subtitle, format);
        Assert.Contains("<c.", lines[0].Text);
        Assert.Contains("<c.", lines[1].Text);

        service.RemoveColorTags(lines, subtitle, format);

        Assert.Equal("Hello", lines[0].Text);
        Assert.Equal("World", lines[1].Text);
    }

    [Fact]
    public void RemoveColorTags_WebVtt_StripsMultipleColorsAcrossLines()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTT();

        var redLine = Line("Hello");
        var blueLine = Line("World");
        service.SetColor(new List<SubtitleLineViewModel> { redLine }, Red, subtitle, format);
        service.SetColor(new List<SubtitleLineViewModel> { blueLine }, Blue, subtitle, format);

        var lines = new List<SubtitleLineViewModel> { redLine, blueLine };
        service.RemoveColorTags(lines, subtitle, format);

        Assert.Equal("Hello", redLine.Text);
        Assert.Equal("World", blueLine.Text);
    }

    [Fact]
    public void RemoveColorTags_WebVtt_StripsDefaultColorClassOnMultipleLinesWithoutHeaderStyle()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTT();

        var text = "<c.yellow>-Qu'est-ce qu'on a ?</c>" + Environment.NewLine + "<c.yellow>-Adrien Dorval, 65 ans.</c>";
        var line = Line(text);

        service.RemoveColorTags(new List<SubtitleLineViewModel> { line }, subtitle, format);

        var expected = "-Qu'est-ce qu'on a ?" + Environment.NewLine + "-Adrien Dorval, 65 ans.";
        Assert.Equal(expected, line.Text);
    }

    [Fact]
    public void RemoveColorTags_Srt_StripsFontColorTags()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new SubRip();

        var lines = new List<SubtitleLineViewModel>
        {
            Line("<font color=\"#FF0000\">Hello</font>"),
        };

        service.RemoveColorTags(lines, subtitle, format);

        Assert.Equal("Hello", lines[0].Text);
    }

    [Fact]
    public void RemoveColorTags_WebVtt_StripsCClassWithDottedContent()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTT();

        var lines = new List<SubtitleLineViewModel>
        {
            Line("<c.white>france.tv access</c>"),
        };

        service.RemoveColorTags(lines, subtitle, format);

        Assert.Equal("france.tv access", lines[0].Text);
    }

    [Fact]
    public void RemoveColorTags_Srt_StripsCClassColorWrapper()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new SubRip();

        var lines = new List<SubtitleLineViewModel>
        {
            Line("<c.white>france.tv access</c>"),
        };

        service.RemoveColorTags(lines, subtitle, format);

        Assert.Equal("france.tv access", lines[0].Text);
    }

    [Fact]
    public void RemoveColorTags_WebVttFileWithLineNumber_StripsCClassColorWrapper()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTTFileWithLineNumber();

        var lines = new List<SubtitleLineViewModel>
        {
            Line("<c.white>france.tv access</c>"),
        };

        service.RemoveColorTags(lines, subtitle, format);

        Assert.Equal("france.tv access", lines[0].Text);
    }

    [Fact]
    public void RemoveColorTags_Assa_StripsAssaColorTags()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new AdvancedSubStationAlpha();

        var lines = new List<SubtitleLineViewModel>
        {
            Line("{\\c&H0000FF&}Hello"),
        };

        service.RemoveColorTags(lines, subtitle, format);

        Assert.Equal("Hello", lines[0].Text);
    }

    // An STL file carries eight teletext colors and Ebu.Save snaps to the nearest one, so a
    // shortcut color the file cannot hold used to be shown in the grid and the video preview and
    // then come out as another color in the file. The tag is snapped when it is written now.
    [Fact]
    public void SetColorTag_Ebu_SnapsToTheNearestTeletextColor()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new Ebu();

        var orange = Color.FromRgb(0xFF, 0xA5, 0x00);

        Assert.Equal("<font color=\"Yellow\">Hello</font>", service.SetColorTag("Hello", orange, subtitle, format));
        Assert.Equal("<font color=\"Red\">Hello</font>", service.SetColorTag("Hello", Red, subtitle, format));
    }

    // The name is the one Ebu's reader writes, so the shortcut toggles off a color that came
    // from a file - it used to compare "#FF0000" against the file's "Red" and never match.
    [Fact]
    public void ContainsColor_Ebu_MatchesTheColorNameFromAFile()
    {
        var service = new ColorService();
        var format = new Ebu();

        Assert.True(service.ContainsColor(Red, "<font color=\"Red\">Hello</font>", format));
        Assert.True(service.ContainsColor(Color.FromRgb(0xFF, 0xA5, 0x00), "<font color=\"Yellow\">Hello</font>", format));
        Assert.False(service.ContainsColor(Blue, "<font color=\"Red\">Hello</font>", format));
    }

    [Fact]
    public void ContainsColor_WebVtt_DetectsAppliedColor()
    {
        var service = new ColorService();
        var subtitle = new Subtitle();
        var format = new WebVTT();

        var line = Line("Hello");
        service.SetColor(new List<SubtitleLineViewModel> { line }, Red, subtitle, format);

        Assert.True(service.ContainsColor(Red, line.Text, format));
        Assert.False(service.ContainsColor(Blue, line.Text, format));
    }
}
