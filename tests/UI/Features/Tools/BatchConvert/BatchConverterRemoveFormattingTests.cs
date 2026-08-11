using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Pins the wiring from <see cref="BatchConvertConfig.RemoveFormattingSettings"/> onto the
/// shared <see cref="RemoveFormattingUtil"/> flags. The removal logic itself moved into libse
/// so seconv's --remove-formatting-rules and this function strip tags identically (#13518);
/// what can still regress on this side is the config-to-flags mapping.
/// </summary>
public class BatchConverterRemoveFormattingTests
{
    private const string InputAss = @"[Script Info]
ScriptType: v4.00+

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,{\pos(10,20)}{\i1}Hi{\i0} {\b1}bold{\b0}
";

    private static async Task<string> ConvertAsync(Action<BatchConvertConfig.RemoveFormattingSettings> configure)
    {
        var dir = Directory.CreateTempSubdirectory("se-remove-formatting-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.ass");
            await File.WriteAllTextAsync(inputFile, InputAss, TestContext.Current.CancellationToken);

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
            config.RemoveFormatting.IsActive = true;
            configure(config.RemoveFormatting);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var outputFile = Path.Combine(outputFolder, "input.ass");
            Assert.True(File.Exists(outputFile), "converted file was not written");

            return Subtitle.Parse(outputFile).Paragraphs[0].Text;
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RemoveAll_StripsEveryTagIncludingPosition()
    {
        Assert.Equal("Hi bold", await ConvertAsync(s => s.RemoveAll = true));
    }

    [Fact]
    public async Task RemoveItalicOnly_KeepsBoldAndPosition()
    {
        Assert.Equal("{\\pos(10,20)}Hi {\\b1}bold{\\b0}", await ConvertAsync(s => s.RemoveItalic = true));
    }

    [Fact]
    public async Task EveryPerKindOption_LeavesPositionTagAlone()
    {
        // The per-kind options together are narrower than RemoveAll: positioning survives.
        var text = await ConvertAsync(s =>
        {
            s.RemoveItalic = true;
            s.RemoveBold = true;
            s.RemoveUnderline = true;
            s.RemoveColor = true;
            s.RemoveFontName = true;
            s.RemoveAlignment = true;
        });

        Assert.Equal("{\\pos(10,20)}Hi bold", text);
    }

    [Fact]
    public async Task NoOptionSelected_LeavesTextUntouched()
    {
        Assert.Equal("{\\pos(10,20)}{\\i1}Hi{\\i0} {\\b1}bold{\\b0}", await ConvertAsync(_ => { }));
    }
}
