using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.Core.Common;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Regression test for the ASSA style-loss family (see PRs #13352/#13353): the
/// split/break-long-lines step rebuilt the subtitle with a bare ToParagraph(), leaving
/// Paragraph.Extra empty - and the ASSA writer takes the Dialogue style column from Extra,
/// so a styled file came out with every line on the first style in the header.
/// </summary>
public class BatchConverterAssaStyleTests
{
    private const string InputAss = @"[Script Info]
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1
Style: Big,Verdana,72,&H0000FFFF,&H0300FFFF,&H00000000,&H02000000,-1,0,0,0,100,100,0,0,1,3,2,8,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Hello there.
Dialogue: 0,0:00:04.00,0:00:06.00,Big,,0,0,0,,Second line.
";

    [Fact]
    public async Task Convert_SplitBreakLongLinesActive_KeepsAssaStyles()
    {
        var dir = Directory.CreateTempSubdirectory("se-assa-style-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.ass");
            await File.WriteAllTextAsync(inputFile, InputAss);

            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var subtitle = Subtitle.Parse(inputFile);
            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new AdvancedSubStationAlpha().Name, subtitle);

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = AdvancedSubStationAlpha.NameOfFormat,
            };

            // Route the subtitle through the rebuild in SplitBreakLongLines without
            // changing any line - the pure pass-through used to drop the styles.
            config.SplitBreakLongLines.IsActive = true;
            config.SplitBreakLongLines.SplitLongLines = false;
            config.SplitBreakLongLines.RebalanceLongLines = false;

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, CancellationToken.None);

            var outputFile = Path.Combine(outputFolder, "input.ass");
            Assert.True(File.Exists(outputFile), "converted file was not written");

            var result = Subtitle.Parse(outputFile);
            Assert.Equal(2, result.Paragraphs.Count);
            Assert.Equal("Default", result.Paragraphs[0].Extra);
            Assert.Equal("Big", result.Paragraphs[1].Extra);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
