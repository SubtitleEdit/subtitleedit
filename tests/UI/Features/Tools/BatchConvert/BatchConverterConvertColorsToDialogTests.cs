using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterConvertColorsToDialogTests
{
    // "Convert colors to dialog" as a batch convert function: a colour change inside a cue marks a
    // change of speaker, so it becomes a dash-prefixed dialog.

    private const string InputSrt = @"1
00:00:01,000 --> 00:00:03,000
<font color=""#ff0000"">Hello there.</font> <font color=""#00ff00"">Hi, how are you?</font>

2
00:00:04,000 --> 00:00:06,000
<font color=""#ff0000"">Just one speaker here.</font>
";

    private static async Task<Subtitle> RunConvertAsync(bool convertActive, Action<BatchConvertConfig>? configure = null)
    {
        var dir = Directory.CreateTempSubdirectory("se-colors-to-dialog-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt);

            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var subtitle = Subtitle.Parse(inputFile);
            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new SubRip().Name, subtitle);

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            };
            config.ConvertColorsToDialog.IsActive = convertActive;
            configure?.Invoke(config);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, CancellationToken.None);

            var outputFile = Path.Combine(outputFolder, "input.srt");
            Assert.True(File.Exists(outputFile), "converted file was not written");
            return Subtitle.Parse(outputFile);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Convert_Active_MakesColorChangeADialogAndRemovesColorTags()
    {
        var result = await RunConvertAsync(convertActive: true);

        Assert.Equal(2, result.Paragraphs.Count);

        var text = result.Paragraphs[0].Text;
        Assert.DoesNotContain("<font", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello there.", text);
        Assert.Contains("Hi, how are you?", text);
        Assert.Contains("-", text);

        // A single colour is not a dialog - nothing to dash.
        Assert.DoesNotContain("-", result.Paragraphs[1].Text);
    }

    [Fact]
    public async Task Convert_ActiveKeepingColorTags_LeavesColorsInPlace()
    {
        var result = await RunConvertAsync(convertActive: true, config => config.ConvertColorsToDialog.RemoveColorTags = false);

        Assert.Contains("<font", result.Paragraphs[0].Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("-", result.Paragraphs[0].Text);
    }

    [Fact]
    public async Task Convert_NotActive_LeavesTextAlone()
    {
        var result = await RunConvertAsync(convertActive: false);

        Assert.Contains("<font color=\"#ff0000\">Hello there.</font>", result.Paragraphs[0].Text);
        Assert.DoesNotContain("-", result.Paragraphs[0].Text);
    }
}
