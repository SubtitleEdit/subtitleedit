using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Tests the headless used-font collection behind the batch convert "Embed fonts"
/// function (and shared with the font collector).
/// </summary>
public class AssaFontEmbedderTests
{
    private static Subtitle LoadAssa()
    {
        var text =
            "[Script Info]\r\n" +
            "ScriptType: v4.00+\r\n" +
            "\r\n" +
            "[V4+ Styles]\r\n" +
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\r\n" +
            "Style: Default,Arial,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1\r\n" +
            "Style: Fancy,My Fancy Font,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1\r\n" +
            "Style: Unused,Unused Font,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1\r\n" +
            "\r\n" +
            "[Events]\r\n" +
            "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\r\n" +
            "Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Hello\r\n" +
            "Dialogue: 0,0:00:04.00,0:00:06.00,Fancy,,0,0,0,,World\r\n" +
            "Dialogue: 0,0:00:07.00,0:00:09.00,Default,,0,0,0,,{\\fn@Inline Font}tagged\r\n";

        var subtitle = new Subtitle();
        new AdvancedSubStationAlpha().LoadSubtitle(subtitle, text.SplitToLines(), string.Empty);
        return subtitle;
    }

    [Fact]
    public void GetUsedFontNames_ReturnsUsedStyleAndInlineFonts()
    {
        var names = AssaFontEmbedder.GetUsedFontNames(LoadAssa());

        Assert.Contains("Arial", names);
        Assert.Contains("My Fancy Font", names);
        Assert.Contains("Inline Font", names); // "@" (vertical variant) prefix is trimmed
    }

    [Fact]
    public void GetUsedFontNames_SkipsFontsOfUnusedStyles()
    {
        var names = AssaFontEmbedder.GetUsedFontNames(LoadAssa());

        Assert.DoesNotContain("Unused Font", names);
    }

    [Fact]
    public void GetUsedFontNames_NoHeader_ReturnsOnlyInlineFonts()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("{\\fnSome Font}Hi", 0, 1000));

        var names = AssaFontEmbedder.GetUsedFontNames(subtitle);

        Assert.Equal(new[] { "Some Font" }, names);
    }

    [Fact]
    public void FindFontFiles_CompletedScan_HasNegativeEntryForUnknownFont()
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            var result = FontHelper.FindFontFiles(
                ["No Such Font"], CancellationToken.None, folders: [folder.FullName]);

            Assert.Single(result);
            Assert.Empty(result["No Such Font"]);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }
}
