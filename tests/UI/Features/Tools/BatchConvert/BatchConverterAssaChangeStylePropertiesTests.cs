using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterAssaChangeStylePropertiesTests
{
    // "Change style properties" edits fields on the styles a file already has - the case it was
    // added for is a translated Arabic subtitle that inherits source styles with letter spacing
    // (issue #14150). Everything else about each style has to survive.

    private const string InputAssa = @"[Script Info]
Title: Test
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,48,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,2,0,1,2,1,2,10,10,20,1
Style: Sign,Verdana,36,&H00FFFF00,&H000000FF,&H00000000,&H00000000,-1,0,0,0,100,100,1.5,0,3,1,0,8,20,20,30,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,{\i1}Hello there.
Dialogue: 0,0:00:04.00,0:00:06.00,Sign,,0,0,0,,A sign.
";

    private static async Task<List<SsaStyle>> RunAsync(Action<BatchConvertConfig.AssaChangeStylePropertiesSettings> configure)
    {
        var dir = Directory.CreateTempSubdirectory("se-assa-style-props-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.ass");
            await File.WriteAllTextAsync(inputFile, InputAssa);

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
            configure(config.AssaChangeStyleProperties);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, CancellationToken.None);

            var outputFile = Path.Combine(outputFolder, "input.ass");
            Assert.True(File.Exists(outputFile), "converted file was not written");
            return AdvancedSubStationAlpha.GetSsaStylesFromHeader(Subtitle.Parse(outputFile).Header);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task SpacingIsZeroedInEveryStyle()
    {
        var styles = await RunAsync(c =>
        {
            c.IsActive = true;
            c.SetSpacing = true;
            c.Spacing = 0;
        });

        Assert.Equal(2, styles.Count);
        Assert.All(styles, s => Assert.Equal(0m, s.Spacing));
    }

    [Fact]
    public async Task InactiveFunctionLeavesSpacingAlone()
    {
        var styles = await RunAsync(c =>
        {
            c.IsActive = false;
            c.SetSpacing = true;
            c.Spacing = 0;
        });

        Assert.Equal(2m, styles.First(s => s.Name == "Default").Spacing);
        Assert.Equal(1.5m, styles.First(s => s.Name == "Sign").Spacing);
    }

    [Fact]
    public async Task AlignmentCodeBecomesTheNumpadDigit()
    {
        var styles = await RunAsync(c =>
        {
            c.IsActive = true;
            c.SetSpacing = false;
            c.SetAlignment = true;
            c.Alignment = "an3";
        });

        Assert.All(styles, s => Assert.Equal("3", s.Alignment));
    }

    [Fact]
    public async Task ABrokenAlignmentCodeChangesNothing()
    {
        var styles = await RunAsync(c =>
        {
            c.IsActive = true;
            c.SetSpacing = true;
            c.Spacing = 0;
            c.SetAlignment = true;
            c.Alignment = "bottom right";
        });

        // The whole function bails out rather than write a style with a broken Alignment field,
        // so the spacing it would otherwise have zeroed is untouched too.
        Assert.Equal(2m, styles.First(s => s.Name == "Default").Spacing);
        Assert.Equal("2", styles.First(s => s.Name == "Default").Alignment);
        Assert.Equal("8", styles.First(s => s.Name == "Sign").Alignment);
    }

    [Fact]
    public async Task TheRestOfEachStyleSurvives()
    {
        var styles = await RunAsync(c =>
        {
            c.IsActive = true;
            c.SetSpacing = true;
            c.Spacing = 0;
            c.SetAlignment = true;
            c.Alignment = "an3";
        });

        var sign = styles.First(s => s.Name == "Sign");
        Assert.Equal("Verdana", sign.FontName);
        Assert.Equal(36m, sign.FontSize);
        Assert.True(sign.Bold);
        Assert.Equal(20, sign.MarginLeft);
        Assert.Equal(30, sign.MarginVertical);
        Assert.Equal("3", sign.BorderStyle);
    }
}
