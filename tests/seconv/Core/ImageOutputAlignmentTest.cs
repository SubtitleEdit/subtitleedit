using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.VobSub;
using SeConv.Core;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// "{\an8}" &amp; co. must position the rendered paragraph in image based output, and must
/// never end up as literal text in the bitmap (issue #13025).
/// </summary>
public class ImageOutputAlignmentTest : IDisposable
{
    private readonly string _tempRoot;

    public ImageOutputAlignmentTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "ImgAlign_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:02,000
        {\an8}Top center

        2
        00:00:03,000 --> 00:00:04,000
        {\an1}Bottom left

        3
        00:00:05,000 --> 00:00:06,000
        Plain bottom center

        """;

    // PlayRes is half the export canvas below, so "\pos" has to be scaled x2 on the way out.
    private const string AssContent = """
        [Script Info]
        ScriptType: v4.00+
        PlayResX: 960
        PlayResY: 540

        [V4+ Styles]
        Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
        Style: Default,Arial,24,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

        [Events]
        Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
        Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,{\an7\pos(50,50)}Anchored top left
        Dialogue: 0,0:00:03.00,0:00:04.00,Default,,0,0,0,,No position tag

        """;

    private async Task<string> ConvertToSup(string content = SrtContent, string extension = ".srt")
    {
        return await Convert(content, extension, "bluraysup", "*.sup", (1920, 1080));
    }

    private async Task<string> Convert(string content, string extension, string format, string outputPattern, (int, int) resolution)
    {
        var input = Path.Combine(_tempRoot, "in" + extension);
        await File.WriteAllTextAsync(input, content);
        var outFolder = Path.Combine(_tempRoot, format);
        Directory.CreateDirectory(outFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
            Resolution = resolution,
            ImageStyle = new ImageExportStyle(),
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        return Assert.Single(Directory.GetFiles(outFolder, outputPattern));
    }

    [Fact]
    public async Task ConvertAsync_BluRaySup_AlignmentTagsPositionTheParagraph()
    {
        var supFile = await ConvertToSup();

        var subtitles = BluRaySupParser.ParseBluRaySup(supFile, new StringBuilder());
        Assert.Equal(3, subtitles.Count);

        var top = subtitles[0].GetPosition();
        var bottomLeft = subtitles[1].GetPosition();
        var untagged = subtitles[2].GetPosition();

        // {\an8} - top of the frame, horizontally centered
        Assert.True(top.Top < 540, $"{{\\an8}} should be in the upper half, was y={top.Top}");

        // {\an1} - bottom of the frame, flush left
        Assert.True(bottomLeft.Top > 540, $"{{\\an1}} should be in the lower half, was y={bottomLeft.Top}");
        Assert.True(bottomLeft.Left < untagged.Left, "{\\an1} should be further left than a centered line");

        // No tag - unchanged default (bottom center)
        Assert.True(untagged.Top > 540, $"untagged line should stay at the bottom, was y={untagged.Top}");
        Assert.True(untagged.Left > 100, "untagged line should stay centered");
    }

    [Fact]
    public async Task ConvertAsync_BluRaySup_PosTagPositionsTheParagraph()
    {
        var supFile = await ConvertToSup(AssContent, ".ass");

        var subtitles = BluRaySupParser.ParseBluRaySup(supFile, new StringBuilder());
        Assert.Equal(2, subtitles.Count);

        // "{\an7\pos(50,50)}" in a 960x540 script, exported at 1920x1080: the top left corner
        // of the text lands at 100,100
        var positioned = subtitles[0].GetPosition();
        Assert.Equal(100, positioned.Left);
        Assert.Equal(100, positioned.Top);

        // A line without "\pos" keeps the default bottom placement
        var untagged = subtitles[1].GetPosition();
        Assert.True(untagged.Top > 540, $"untagged line should stay at the bottom, was y={untagged.Top}");
    }

    [Fact]
    public async Task ConvertAsync_VobSub_PosTagPositionsTheParagraph()
    {
        // VobSub is 720x576 (PAL), so the 960x540 script's "\pos(50,50)" scales to
        // 50*720/960 = 37.5 -> 38 and 50*576/540 = 53.3 -> 53
        var subFile = await Convert(AssContent, ".ass", "vobsub", "*.sub", (720, 576));

        var parser = new VobSubParser(true);
        parser.OpenSubIdx(subFile, Path.ChangeExtension(subFile, ".idx"));
        var packs = parser.MergeVobSubPacks();
        Assert.Equal(2, packs.Count);

        // Decoding fills in ImageDisplayArea from the subpicture's display control commands
        foreach (var pack in packs)
        {
            pack.GetBitmap();
        }

        var positioned = packs[0].SubPicture.ImageDisplayArea;
        Assert.Equal(38, positioned.Left);
        Assert.Equal(53, positioned.Top);

        var untagged = packs[1].SubPicture.ImageDisplayArea;
        Assert.True(untagged.Top > 288, $"untagged line should stay at the bottom, was y={untagged.Top}");
    }
}
