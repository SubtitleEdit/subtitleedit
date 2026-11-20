using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// "Delete lines with actors or styles" compared the names with Paragraph.Style only, but ASSA
/// keeps a line's style in Paragraph.Extra - so an .ass file came out unchanged (#17057).
/// </summary>
public class BatchConverterDeleteLinesTests
{
    private const string InputAss = @"[Script Info]
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1
Style: Flashback,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1
Style: Signs,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,Keep me.
Dialogue: 0,0:00:03.00,0:00:04.00,Flashback,,0,0,0,,Delete flashback.
Dialogue: 0,0:00:05.00,0:00:06.00,Signs,,0,0,0,,Delete sign.
Dialogue: 0,0:00:07.00,0:00:08.00,Default,Bob,0,0,0,,Delete Bob.
Dialogue: 0,0:00:09.00,0:00:10.00,Default,,0,0,0,,Keep me too.
";

    [Fact]
    public async Task DeleteLines_AssaStylesAndActorsAreDeleted()
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-delete-lines-test");
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
                DeleteLines = new BatchConvertConfig.DeleteLinesSettings
                {
                    IsActive = true,
                    DeleteActorsOrStyles = "flashback, Signs;bob",
                },
            });

            var subtitle = new Subtitle();
            var format = new AdvancedSubStationAlpha();
            format.LoadSubtitle(subtitle, InputAss.SplitToLines(), inputFile);
            subtitle.OriginalFormat = format;
            var item = new BatchConvertItem(inputFile, 1, format.Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var raw = await File.ReadAllTextAsync(Path.Combine(outputDir.FullName, "movie.ass"), TestContext.Current.CancellationToken);
            Assert.Contains("Keep me.", raw);
            Assert.Contains("Keep me too.", raw);
            Assert.DoesNotContain("Delete flashback.", raw);
            Assert.DoesNotContain("Delete sign.", raw);
            Assert.DoesNotContain("Delete Bob.", raw);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
