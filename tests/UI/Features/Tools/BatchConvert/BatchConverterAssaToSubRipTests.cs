using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch converting .ass to .srt must convert the ASSA tags exactly like File - Save as does
/// (#15412): the native formatting was removed from the source item instead of the copy that
/// got written, so the .srt kept raw {\i1} and one {\an8} per line.
/// </summary>
public class BatchConverterAssaToSubRipTests
{
    private const string InputAss = @"[Script Info]
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,{\an8}First line\N{\an8}second line
Dialogue: 0,0:00:04.00,0:00:06.00,Default,,0,0,0,,{\i1}A ukázat, že je jedním z nich.{\i}
";

    [Fact]
    public async Task AssToSrt_ConvertsItalicAndKeepsSingleAlignmentTag()
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-ass-srt-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.ass");
            await File.WriteAllTextAsync(inputFile, InputAss, TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                SaveInSourceFolder = true,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            });

            var subtitle = Subtitle.Parse(inputFile);
            Assert.Equal(AdvancedSubStationAlpha.NameOfFormat, subtitle.OriginalFormat.Name);
            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, subtitle.OriginalFormat.Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var output = await File.ReadAllTextAsync(Path.Combine(dir.FullName, "input.srt"), TestContext.Current.CancellationToken);
            var srt = new Subtitle();
            new SubRip().LoadSubtitle(srt, output.SplitToLines(), null);

            Assert.Equal(2, srt.Paragraphs.Count);
            Assert.Equal("{\\an8}First line" + Environment.NewLine + "second line", srt.Paragraphs[0].Text);
            Assert.Equal("<i>A ukázat, že je jedním z nich.</i>", srt.Paragraphs[1].Text);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
