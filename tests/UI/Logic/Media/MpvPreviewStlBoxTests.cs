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

    private static string BuildPreviewText(bool useBox)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var oldUseBox = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
        var oldFontName = Se.Settings.Video.MpvPreviewFontName;
        var oldFontSize = Se.Settings.Video.MpvPreviewFontSize;
        try
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = useBox;
            Se.Settings.Video.MpvPreviewFontName = "Verdana";
            Se.Settings.Video.MpvPreviewFontSize = 33;

            var subtitle = new Subtitle { Header = MakeStlHeader("1") }; // 1 = level 1 teletext
            subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000));

            var reloader = new MpvReloader();
            var method = typeof(MpvReloader).GetMethod("BuildPreviewText", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            var result = method!.Invoke(reloader, new object?[] { subtitle, null, typeof(Ebu), 0, string.Empty })!;
            return (string)result.GetType().GetField("Item2")!.GetValue(result)!;
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = oldUseBox;
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
        var line = assText.Split('\n').FirstOrDefault(l => l.StartsWith("Dialogue:", StringComparison.Ordinal));
        Assert.NotNull(line);
        return line!.Split(',')[3];
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
}
