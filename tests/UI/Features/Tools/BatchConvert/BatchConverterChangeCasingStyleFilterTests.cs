using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch "Change casing" can be limited to lines with given actors or styles (#17072).
/// </summary>
public class BatchConverterChangeCasingStyleFilterTests
{
    private const string InputAss = @"[Script Info]
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1
Style: Signs,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,Keep my casing.
Dialogue: 0,0:00:03.00,0:00:04.00,Signs,,0,0,0,,Sign text here.
Dialogue: 0,0:00:05.00,0:00:06.00,Default,Bob,0,0,0,,Bob talks.
Dialogue: 0,0:00:07.00,0:00:08.00,Default,,0,0,0,,Keep me too.
";

    private static async Task<string> Convert(string onlyActorsOrStyles)
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-casing-filter-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.ass");
            await File.WriteAllTextAsync(inputFile, InputAss, TestContext.Current.CancellationToken);

            var outputDir = Directory.CreateDirectory(Path.Combine(dir.FullName, "out"));
            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputDir.FullName,
                Overwrite = true,
                TargetFormatName = new AdvancedSubStationAlpha().Name,
                ChangeCasing = new BatchConvertConfig.ChangeCasingSettings
                {
                    IsActive = true,
                    AllUppercase = true,
                    OnlyActorsOrStyles = onlyActorsOrStyles,
                },
            });

            var subtitle = new Subtitle();
            var format = new AdvancedSubStationAlpha();
            format.LoadSubtitle(subtitle, InputAss.SplitToLines(), inputFile);
            subtitle.OriginalFormat = format;
            var item = new BatchConvertItem(inputFile, 1, format.Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            return await File.ReadAllTextAsync(Path.Combine(outputDir.FullName, "movie.ass"), TestContext.Current.CancellationToken);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ChangeCasing_OnlyMatchingStylesAndActorsChange()
    {
        var raw = await Convert("signs; bob");

        Assert.Contains("SIGN TEXT HERE.", raw);
        Assert.Contains("BOB TALKS.", raw);
        Assert.Contains("Keep my casing.", raw);
        Assert.Contains("Keep me too.", raw);
    }

    [Fact]
    public async Task ChangeCasing_EmptyFilterChangesAllLines()
    {
        var raw = await Convert(string.Empty);

        Assert.Contains("KEEP MY CASING.", raw);
        Assert.Contains("SIGN TEXT HERE.", raw);
        Assert.Contains("KEEP ME TOO.", raw);
    }

    [Fact]
    public async Task ChangeCasing_NoMatchingLinesLeavesFileUnchanged()
    {
        var raw = await Convert("NoSuchStyle");

        Assert.Contains("Keep my casing.", raw);
        Assert.Contains("Sign text here.", raw);
    }
}
