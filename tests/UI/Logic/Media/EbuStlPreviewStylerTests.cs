using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;
using System.Text;

namespace UITests.Logic.Media;

/// <summary>
/// The mpv and the VLC reloader used to carry a copy each of the EBU STL preview styling, and only
/// the mpv copy was ever fixed - the VLC one still drew the box in a hard coded 12 pt Tahoma that
/// threw away the font, size, color and margins the preview was set up with. Both go through
/// <see cref="EbuStlPreviewStyler"/> now, so these cover the VLC preview too.
/// </summary>
public class EbuStlPreviewStylerTests
{
    private const int NameField = 0;
    private const int FontNameField = 1;
    private const int FontSizeField = 2;
    private const int ScaleYField = 12;
    private const int BorderStyleField = 15;

    public EbuStlPreviewStylerTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static string MakeStlHeader(string displayStandardCode)
    {
        var buffer = new byte[1024];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = 0x20;
        }

        Encoding.ASCII.GetBytes("850").CopyTo(buffer, 0);
        Encoding.ASCII.GetBytes("STL25.01").CopyTo(buffer, 3);
        Encoding.ASCII.GetBytes(displayStandardCode).CopyTo(buffer, 11);
        Encoding.ASCII.GetBytes("00").CopyTo(buffer, 12);
        return Ebu.ReadHeader(buffer).ToString();
    }

    private static Subtitle Style(string displayStandardCode, bool useBox, bool useDoubleHeight, string text = "Hello world")
    {
        var settings = Configuration.Settings.SubtitleSettings;
        var oldUseBox = settings.EbuStlTeletextUseBox;
        var oldUseDoubleHeight = settings.EbuStlTeletextUseDoubleHeight;
        try
        {
            settings.EbuStlTeletextUseBox = useBox;
            settings.EbuStlTeletextUseDoubleHeight = useDoubleHeight;

            var header = MakeStlHeader(displayStandardCode);
            var subtitle = new Subtitle { Header = header };
            subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000));

            var previewStyle = new SsaStyle { Name = "Default", FontName = "Verdana", FontSize = 33 };
            EbuStlPreviewStyler.Apply(subtitle, header, previewStyle, "preview");

            return subtitle;
        }
        finally
        {
            settings.EbuStlTeletextUseDoubleHeight = oldUseDoubleHeight;
            settings.EbuStlTeletextUseBox = oldUseBox;
        }
    }

    private static string[] GetStyle(Subtitle subtitle, string name)
    {
        var line = subtitle.Header.SplitToLines().FirstOrDefault(l => l.StartsWith("Style: " + name + ",", StringComparison.Ordinal));
        Assert.NotNull(line);
        var fields = line!.Trim().Split(',');
        fields[NameField] = fields[NameField].Substring("Style: ".Length);
        return fields;
    }

    [Fact]
    public void BothStylesFollowThePreviewFontAndSize()
    {
        var subtitle = Style("1", useBox: true, useDoubleHeight: false);

        foreach (var name in new[] { "Box", "Default" })
        {
            var style = GetStyle(subtitle, name);
            Assert.Equal("Verdana", style[FontNameField]);
            Assert.Equal("33", style[FontSizeField]);
        }
    }

    [Fact]
    public void TheBoxIsTheOnlyStyleDrawnWithAnOpaqueBackground()
    {
        var subtitle = Style("1", useBox: true, useDoubleHeight: false);

        Assert.Equal("3", GetStyle(subtitle, "Box")[BorderStyleField]);
        Assert.NotEqual("3", GetStyle(subtitle, "Default")[BorderStyleField]);
    }

    [Theory]
    [InlineData("1", true, "Box")] // level-1 teletext
    [InlineData("2", true, "Box")] // level-2 teletext
    [InlineData("1", false, "Default")]
    [InlineData("0", true, "Default")] // open subtitling has no teletext control codes
    public void TheBoxStyleIsUsedOnlyWhenTheFileCarriesBoxes(string displayStandardCode, bool useBox, string expected)
    {
        var subtitle = Style(displayStandardCode, useBox, useDoubleHeight: false);

        Assert.Equal(expected, subtitle.Paragraphs[0].Extra);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DoubleHeightStretchesBoxedAndUnboxedLinesAlike(bool useBox)
    {
        var subtitle = Style("1", useBox, useDoubleHeight: true);

        Assert.Equal("200", GetStyle(subtitle, "Box")[ScaleYField]);
        Assert.Equal("200", GetStyle(subtitle, "Default")[ScaleYField]);
    }

    [Theory]
    [InlineData("1", false)] // teletext, double height turned off
    [InlineData("0", true)] // open subtitling - written without the double height code
    public void SingleHeightLeavesTheGlyphsAlone(string displayStandardCode, bool useDoubleHeight)
    {
        var subtitle = Style(displayStandardCode, useBox: true, useDoubleHeight: useDoubleHeight);

        Assert.Equal("100", GetStyle(subtitle, "Box")[ScaleYField]);
        Assert.Equal("100", GetStyle(subtitle, "Default")[ScaleYField]);
    }

    // A single line may ask for the box itself, whatever the file-wide setting says.
    [Fact]
    public void ABoxTagInTheTextWinsAndIsStripped()
    {
        var subtitle = Style("1", useBox: false, useDoubleHeight: false, text: "<box>Hello world</box>");

        Assert.Equal("Box", subtitle.Paragraphs[0].Extra);
        Assert.Equal("Hello world", subtitle.Paragraphs[0].Text);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("short", false)]
    [InlineData("850STL25.01 and then some padding to get past twenty characters", true)]
    public void OnlyAGsiBlockIsTreatedAsStl(string header, bool expected)
    {
        Assert.Equal(expected, EbuStlPreviewStyler.IsStlHeader(header));
    }
}
