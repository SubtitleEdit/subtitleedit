using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterSplitBreakLongLinesTests
{
    // Issue #15398: batch "Split/rebalance long lines" now has the dialog's "only subtitles with
    // a too-long line" and "unbreak lines shorter than" options, and caps the unbreak threshold
    // at the single line max length like the dialog does (#12910).

    private static async Task<Subtitle> RunConvertAsync(string text, Action<BatchConvertConfig.SplitBreakLongLinesSettings> configure)
    {
        var dir = Directory.CreateTempSubdirectory("se-splitbreak-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(inputFile, "1\n00:00:01,000 --> 00:00:04,000\n" + text + "\n");

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
            config.SplitBreakLongLines.IsActive = true;
            config.SplitBreakLongLines.RebalanceLongLines = true;
            config.SplitBreakLongLines.SingleLineMaxLength = 42;
            config.SplitBreakLongLines.MaxNumberOfLines = 2;
            configure(config.SplitBreakLongLines);

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
    public async Task Rebalance_OnlyLinesTooLong_KeepsLineBreaksWhenAllLinesFit()
    {
        const string text = "Well...\nI think we should go home now.";
        var result = await RunConvertAsync(text, c =>
        {
            c.RebalanceOnlyLinesTooLong = true;
            c.UnbreakLinesShorterThan = 10;
        });

        Assert.Equal(text, result.Paragraphs[0].Text.Replace("\r\n", "\n"));
    }

    [Fact]
    public async Task Rebalance_AllLines_RewrapsUnbalancedLines()
    {
        const string text = "Well...\nI think we should go home now.";
        var result = await RunConvertAsync(text, c =>
        {
            c.RebalanceOnlyLinesTooLong = false;
            c.UnbreakLinesShorterThan = 10;
        });

        Assert.NotEqual(text, result.Paragraphs[0].Text.Replace("\r\n", "\n"));
    }

    [Fact]
    public async Task Rebalance_OnlyLinesTooLong_StillFixesTooLongLine()
    {
        var result = await RunConvertAsync("This line is far too long to fit within the limit set.", c =>
        {
            c.RebalanceOnlyLinesTooLong = true;
            c.UnbreakLinesShorterThan = 10;
        });

        var lines = result.Paragraphs[0].Text.SplitToLines();
        Assert.Equal(2, lines.Count);
        Assert.All(lines, l => Assert.True(l.Length <= 42, $"line too long: '{l}'"));
    }

    [Fact]
    public async Task Rebalance_UnbreakThresholdAboveMaxLength_DoesNotMergeBeyondMaxLength()
    {
        // #12910: an unbreak threshold above the single line max length must not merge text
        // onto one line that is longer than the max length.
        var result = await RunConvertAsync("This text is longer than\ntwenty characters.", c =>
        {
            c.SingleLineMaxLength = 30;
            c.UnbreakLinesShorterThan = 100;
        });

        var lines = result.Paragraphs[0].Text.SplitToLines();
        Assert.Equal(2, lines.Count);
        Assert.All(lines, l => Assert.True(l.Length <= 30, $"line too long: '{l}'"));
    }
}
