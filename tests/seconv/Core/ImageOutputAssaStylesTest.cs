using Nikse.SubtitleEdit.Core.BluRaySup;
using SeConv.Core;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// ASSA/SSA input is drawn with each line's own style - here the alignment and size of a
/// "Sign" style - unless <see cref="ImageExportStyle.UseAssaStyles"/> is off (--no-assa-styles).
/// </summary>
public class ImageOutputAssaStylesTest : IDisposable
{
    private readonly string _tempRoot;

    public ImageOutputAssaStylesTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "ImgAssaStyles_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private const string AssContent = """
        [Script Info]
        ScriptType: v4.00+
        PlayResX: 960
        PlayResY: 540

        [V4+ Styles]
        Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
        Style: Default,Arial,24,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1
        Style: Sign,Arial,48,&H0000FFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,8,10,10,10,1

        [Events]
        Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
        Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,Dialogue line
        Dialogue: 0,0:00:03.00,0:00:04.00,Sign,,0,0,0,,Sign line

        """;

    private async Task<List<BluRaySupParser.PcsData>> ConvertToSup(bool useAssaStyles)
    {
        var input = Path.Combine(_tempRoot, "in.ass");
        await File.WriteAllTextAsync(input, AssContent);
        var outFolder = Path.Combine(_tempRoot, useAssaStyles ? "styles" : "nostyles");
        Directory.CreateDirectory(outFolder);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "bluraysup",
            OutputFolder = outFolder,
            Overwrite = true,
            Resolution = (1920, 1080),
            ImageStyle = new ImageExportStyle { UseAssaStyles = useAssaStyles },
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var supFile = Assert.Single(Directory.GetFiles(outFolder, "*.sup"));
        return BluRaySupParser.ParseBluRaySup(supFile, new StringBuilder());
    }

    [Fact]
    public async Task ConvertAsync_AssaInput_UsesEachLinesStyle()
    {
        var subtitles = await ConvertToSup(useAssaStyles: true);
        Assert.Equal(2, subtitles.Count);

        var dialogue = subtitles[0].GetPosition();
        var sign = subtitles[1].GetPosition();

        // "Sign" is \an8 with MarginV 10 in a 540 line script: 20 px from the top at 1080
        Assert.Equal(20, sign.Top);
        Assert.True(dialogue.Top > 540, $"Default style should be at the bottom, was y={dialogue.Top}");

        // Twice the font size of Default
        Assert.True(subtitles[1].GetBitmap().Height > subtitles[0].GetBitmap().Height * 1.6);
    }

    [Fact]
    public async Task ConvertAsync_NoAssaStyles_UsesTheImageStyle()
    {
        var subtitles = await ConvertToSup(useAssaStyles: false);
        Assert.Equal(2, subtitles.Count);

        // Both lines at the bottom in the same font
        Assert.True(subtitles[1].GetPosition().Top > 540);
        Assert.Equal(subtitles[0].GetBitmap().Height, subtitles[1].GetBitmap().Height);
    }
}
