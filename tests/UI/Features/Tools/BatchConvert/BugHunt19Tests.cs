using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System.Linq;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Guard tests for the 2026-08-27 bug hunt (sweep 19), which compared each batch-convert step
/// against the dialog that owns the same operation. The batch side is the one nobody watches
/// run, so a step that quietly does the wrong thing - or nothing - can survive a long time.
/// </summary>
public class BugHunt19Tests
{
    private const string InputSrt = @"1
00:00:00,000 --> 00:00:01,000
Hello there.

2
00:00:01,000 --> 00:00:02,000
Second line.
";

    private static async Task<Subtitle> RunAsync(string input, Action<BatchConvertConfig> configure)
    {
        var dir = Directory.CreateTempSubdirectory("se-bughunt19");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(inputFile, input, TestContext.Current.CancellationToken);

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
            configure(config);

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
    public async Task ChangeFrameRate_KeepsEqualLengthCuesEqual()
    {
        // Scaling start and end independently rounds the two ends apart: at 25 -> 23.976 the
        // first 1000 ms cue came out 1043 ms and the second - same length, one second later -
        // came out 1042 ms. The dialog was fixed for #14056 to scale start plus duration; batch
        // still called Subtitle.ChangeFrameRate and kept the fractional times.
        var result = await RunAsync(InputSrt, config =>
        {
            config.ChangeFrameRate.IsActive = true;
            config.ChangeFrameRate.FromFrameRate = 25;
            config.ChangeFrameRate.ToFrameRate = 23.976;
        });

        Assert.Equal(2, result.Paragraphs.Count);
        Assert.Equal(
            result.Paragraphs[0].DurationTotalMilliseconds,
            result.Paragraphs[1].DurationTotalMilliseconds);

        foreach (var p in result.Paragraphs)
        {
            Assert.Equal(p.StartTime.TotalMilliseconds, Math.Round(p.StartTime.TotalMilliseconds));
            Assert.Equal(p.EndTime.TotalMilliseconds, Math.Round(p.EndTime.TotalMilliseconds));
        }
    }

    [Fact]
    public async Task ChangeFrameRate_DoesNotManufactureAnOverlap()
    {
        var result = await RunAsync(InputSrt, config =>
        {
            config.ChangeFrameRate.IsActive = true;
            config.ChangeFrameRate.FromFrameRate = 25;
            config.ChangeFrameRate.ToFrameRate = 23.976;
        });

        Assert.True(
            result.Paragraphs[0].EndTime.TotalMilliseconds <= result.Paragraphs[1].StartTime.TotalMilliseconds,
            "the rounding turned two touching cues into an overlap");
    }

    [Fact]
    public async Task ChangeSpeed_ZeroPercent_LeavesTheSubtitleAlone()
    {
        // 100/0 is infinity, and TimeSpan.FromMilliseconds(infinity) throws - one bad value in a
        // saved batch profile took down the whole run. The dialog's spinner starts at 1, but the
        // batch config is deserialized from JSON, so the UI cannot be the guard.
        var result = await RunAsync(InputSrt, config =>
        {
            config.ChangeSpeed.IsActive = true;
            config.ChangeSpeed.SpeedPercent = 0;
        });

        Assert.Equal(2, result.Paragraphs.Count);
        Assert.Equal(0, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(1000, result.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void FixNamesLogic_FindsNamesWithWrongCasing()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("i met joe in paris.", 0, 2000));

        var names = FixNamesLogic.FindNames(subtitle, ["Joe", "Paris", "Unused"], string.Empty, "en_US");

        Assert.Contains(names, n => n.Name == "Joe");
        Assert.Contains(names, n => n.Name == "Paris");
        Assert.DoesNotContain(names, n => n.Name == "Unused");
    }

    [Fact]
    public void FixNamesLogic_LeavesCommonWordsUnchecked()
    {
        // "Bill", "Rose", "Lane" and "US" are ordinary words as often as they are names, so the
        // dialog offers them unchecked. An unattended batch run must make the same choice.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("pay the bill, joe.", 0, 2000));

        var names = FixNamesLogic.FindNames(subtitle, ["Bill", "Joe"], string.Empty, "en_US");

        Assert.False(names.Single(n => n.Name == "Bill").IsChecked);
        Assert.True(names.Single(n => n.Name == "Joe").IsChecked);
    }

    [Fact]
    public void FixNamesLogic_ApplyNames_FixesCasing()
    {
        var result = FixNamesLogic.ApplyNames("i met joe in paris.", ["Joe", "Paris"]);

        Assert.Equal("i met Joe in Paris.", result);
    }

    [Fact]
    public void FixNamesLogic_ApplyNames_SkipsAnAllUppercaseLine()
    {
        var result = FixNamesLogic.ApplyNames("I MET JOE IN PARIS.", ["Joe", "Paris"]);

        Assert.Equal("I MET JOE IN PARIS.", result);
    }
}
