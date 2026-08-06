using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterBeautifyTimeCodesTests
{
    // Issue #13224: "Beautify time codes" as a batch convert function. Without a matching
    // video file the converter falls back to the default frame rate, so the observable
    // effect is frame alignment plus the profile's minimum gap between cues.

    private const string InputSrt = @"1
00:00:01,013 --> 00:00:03,987
Hello there.

2
00:00:04,001 --> 00:00:06,499
Second line.

3
00:00:06,530 --> 00:00:09,123
Third line.
";

    private static async Task<Subtitle> RunConvertAsync(bool beautifyActive, Action<BatchConvertConfig>? configure = null)
    {
        var dir = Directory.CreateTempSubdirectory("se-beautify-test");
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
            config.BeautifyTimeCodes.IsActive = beautifyActive;
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
    public async Task Convert_BeautifyTimeCodesActive_AlignsCuesToFramesAndEnforcesMinGap()
    {
        var result = await RunConvertAsync(beautifyActive: true);

        Assert.Equal(3, result.Paragraphs.Count);

        var frameRate = Configuration.Settings.General.DefaultFrameRate;
        foreach (var p in result.Paragraphs)
        {
            foreach (var ms in new[] { p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds })
            {
                var frames = ms * frameRate / 1000.0;
                Assert.True(Math.Abs(frames - Math.Round(frames)) < 0.1,
                    $"time code {ms} ms is not aligned to a {frameRate} fps frame");
            }
        }

        // Default profile enforces a minimum gap (in frames) between consecutive cues.
        var minGapMs = Configuration.Settings.BeautifyTimeCodes.Profile.Gap * 1000.0 / frameRate - 0.5;
        for (var i = 0; i < result.Paragraphs.Count - 1; i++)
        {
            var gap = result.Paragraphs[i + 1].StartTime.TotalMilliseconds - result.Paragraphs[i].EndTime.TotalMilliseconds;
            Assert.True(gap >= minGapMs, $"gap between cue {i + 1} and {i + 2} is {gap} ms, expected at least {minGapMs} ms");
        }
    }

    [Fact]
    public async Task Convert_BeautifyTimeCodesFixedFrameRate_AlignsCuesToFixedFrameRate()
    {
        // 25 fps => frames are 40 ms apart, which no fallback rate (23.976/24/30/...)
        // would produce, so alignment proves the fixed rate was used.
        var result = await RunConvertAsync(beautifyActive: true, config =>
        {
            config.BeautifyTimeCodes.UseFixedFrameRate = true;
            config.BeautifyTimeCodes.FixedFrameRate = 25;
        });

        Assert.Equal(3, result.Paragraphs.Count);

        foreach (var p in result.Paragraphs)
        {
            foreach (var ms in new[] { p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds })
            {
                var frames = ms * 25.0 / 1000.0;
                Assert.True(Math.Abs(frames - Math.Round(frames)) < 0.1,
                    $"time code {ms} ms is not aligned to a 25 fps frame");
            }
        }
    }

    [Fact]
    public async Task Convert_BeautifyTimeCodesInactive_LeavesTimeCodesUnchanged()
    {
        var result = await RunConvertAsync(beautifyActive: false);

        Assert.Equal(3, result.Paragraphs.Count);
        Assert.Equal(1013, result.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3987, result.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal(4001, result.Paragraphs[1].StartTime.TotalMilliseconds, 3);
        Assert.Equal(6499, result.Paragraphs[1].EndTime.TotalMilliseconds, 3);
        Assert.Equal(6530, result.Paragraphs[2].StartTime.TotalMilliseconds, 3);
        Assert.Equal(9123, result.Paragraphs[2].EndTime.TotalMilliseconds, 3);
    }
}
