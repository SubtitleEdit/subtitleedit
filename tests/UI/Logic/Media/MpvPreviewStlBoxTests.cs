using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Reflection;
using System.Text;

namespace UITests.Logic.Media;

/// <summary>
/// An EBU STL file that uses the teletext box is previewed with an opaque background behind the
/// text. The box style used to be hard coded to 12 pt Tahoma, so a boxed line ignored the font,
/// size, color, alignment and margins the mpv preview was set up with and came out much smaller
/// than an unboxed one.
/// </summary>
public class MpvPreviewStlBoxTests
{
    private const int FontNameField = 1;
    private const int FontSizeField = 2;
    private const int ScaleYField = 12;
    private const int BorderStyleField = 15;

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

    private static string BuildPreviewText(bool useBox, string displayStandardCode = "1", bool useDoubleHeight = false, Type? uiFormatType = null)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var oldUseBox = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
        var oldUseDoubleHeight = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight;
        var oldFontName = Se.Settings.Video.MpvPreviewFontName;
        var oldFontSize = Se.Settings.Video.MpvPreviewFontSize;
        try
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = useBox;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = useDoubleHeight;
            Se.Settings.Video.MpvPreviewFontName = "Verdana";
            Se.Settings.Video.MpvPreviewFontSize = 33;

            var subtitle = new Subtitle { Header = MakeStlHeader(displayStandardCode) };
            subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000) { MarginV = "20" });

            var format = (SubtitleFormat)Activator.CreateInstance(uiFormatType ?? typeof(Ebu))!;
            var reloader = new MpvReloader();
            var method = typeof(MpvReloader).GetMethod("BuildPreviewText", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            var result = method!.Invoke(reloader, new object?[] { subtitle, null, format.GetType(), format.HasPositionSupport, 0, string.Empty })!;
            return (string)result.GetType().GetField("Item2")!.GetValue(result)!;
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = oldUseBox;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = oldUseDoubleHeight;
            Se.Settings.Video.MpvPreviewFontName = oldFontName;
            Se.Settings.Video.MpvPreviewFontSize = oldFontSize;
        }
    }

    private static string[] GetStyle(string assText, string name)
    {
        var line = assText.Split('\n').FirstOrDefault(l => l.StartsWith("Style: " + name + ",", StringComparison.Ordinal));
        Assert.NotNull(line);
        var fields = line!.Trim().Split(',');
        fields[0] = fields[0].Substring("Style: ".Length);
        return fields;
    }

    private static string GetDialogueStyle(string assText)
    {
        return GetDialogueField(assText, 3);
    }

    // Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
    private static string GetDialogueMarginV(string assText)
    {
        return GetDialogueField(assText, 7);
    }

    private static string GetDialogueField(string assText, int index)
    {
        var line = assText.Split('\n').FirstOrDefault(l => l.StartsWith("Dialogue:", StringComparison.Ordinal));
        Assert.NotNull(line);
        return line!.Split(',')[index].Trim();
    }

    [AvaloniaFact]
    public void BoxStyleFollowsThePreviewFontAndSize()
    {
        var box = GetStyle(BuildPreviewText(useBox: true), "Box");

        Assert.Equal("Verdana", box[FontNameField]);
        Assert.Equal("33", box[FontSizeField]);
    }

    [AvaloniaFact]
    public void BoxStyleDiffersFromTheDefaultOnlyInHowTheBackgroundIsDrawn()
    {
        var assText = BuildPreviewText(useBox: true);
        var box = GetStyle(assText, "Box");
        var defaultStyle = GetStyle(assText, "Default");

        Assert.Equal("3", box[BorderStyleField]); // opaque box
        Assert.NotEqual("3", defaultStyle[BorderStyleField]);

        // Everything that is not about the background has to match, or a boxed line jumps to
        // another font, size, color or position than an unboxed one.
        for (var i = 0; i < box.Length; i++)
        {
            if (i == 0 || i == BorderStyleField || i == 17) // name, border style, shadow width
            {
                continue;
            }

            Assert.Equal(defaultStyle[i], box[i]);
        }
    }

    [AvaloniaFact]
    public void TeletextLinesUseTheBoxStyleOnlyWhenTheFileUsesBoxes()
    {
        Assert.Equal("Box", GetDialogueStyle(BuildPreviewText(useBox: true)));
        Assert.Equal("Default", GetDialogueStyle(BuildPreviewText(useBox: false)));
    }

    // The box is a teletext control code, so Ebu.Save writes none for open subtitling however the
    // save options dialog is set - and a preview that drew one anyway would promise a box the
    // file never carries (user report on PR #14228).
    [AvaloniaFact]
    public void OpenSubtitlingNeverUsesTheBoxStyle()
    {
        Assert.Equal("Default", GetDialogueStyle(BuildPreviewText(useBox: true, displayStandardCode: "0")));
    }

    // A teletext double height row is the same glyphs at twice the height. The code is written per
    // text field (Ebu.EncodeText), so a file that uses it uses it for every line - boxed or not.
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void DoubleHeightStretchesTheLineVertically(bool useBox)
    {
        var assText = BuildPreviewText(useBox: useBox, useDoubleHeight: true);

        Assert.Equal("200", GetStyle(assText, "Box")[ScaleYField]);
        Assert.Equal("200", GetStyle(assText, "Default")[ScaleYField]);
    }

    [AvaloniaFact]
    public void SingleHeightLeavesTheLineAlone()
    {
        var assText = BuildPreviewText(useBox: true, useDoubleHeight: false);

        Assert.Equal("100", GetStyle(assText, "Box")[ScaleYField]);
        Assert.Equal("100", GetStyle(assText, "Default")[ScaleYField]);
    }

    // Double height is a teletext control code, so open subtitling is written without it.
    [AvaloniaFact]
    public void OpenSubtitlingNeverUsesDoubleHeight()
    {
        var assText = BuildPreviewText(useBox: true, displayStandardCode: "0", useDoubleHeight: true);

        Assert.Equal("100", GetStyle(assText, "Default")[ScaleYField]);
    }

    // The GSI block and the teletext row in MarginV stay on the subtitle when the format is
    // switched in the toolbar, and the preview kept drawing the box, the double height and the
    // rows of a file the subtitle is no longer shown as.
    [AvaloniaFact]
    public void SwitchingTheFormatToSubRipDropsTheTeletextStyling()
    {
        var assText = BuildPreviewText(useBox: true, useDoubleHeight: true, uiFormatType: typeof(SubRip));

        Assert.DoesNotContain("Style: Box,", assText, StringComparison.Ordinal);
        Assert.Equal("Default", GetDialogueStyle(assText));
        Assert.Equal("100", GetStyle(assText, "Default")[ScaleYField]);
    }

    // A teletext row is not a pixel margin - left behind it moved every line by a near random
    // amount, so it has to be stripped even though nothing is positioned any more.
    [AvaloniaFact]
    public void SwitchingTheFormatToSubRipLeavesNoTeletextRowBehind()
    {
        Assert.NotEqual("0", GetDialogueMarginV(BuildPreviewText(useBox: false)));
        Assert.Equal("0", GetDialogueMarginV(BuildPreviewText(useBox: false, uiFormatType: typeof(SubRip))));
    }
}
