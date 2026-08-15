using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterSnapTimeCodesToFramesTests
{
    // "Snap all times to frames" as a batch convert function: every start/end time is rounded to
    // the nearest frame. 25 fps is used here so a frame is exactly 40 ms and the expected values
    // survive the millisecond precision of SubRip.

    private const string InputSrt = @"1
00:00:01,013 --> 00:00:03,987
Hello there.

2
00:00:04,001 --> 00:00:06,499
Second line.

3
00:00:10,001 --> 00:00:10,009
Shorter than a frame.
";

    private static async Task<Subtitle> RunConvertAsync(bool snapActive, double fixedFrameRate = 25)
    {
        var dir = Directory.CreateTempSubdirectory("se-snap-frames-test");
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
            config.SnapTimeCodesToFrames.IsActive = snapActive;
            config.SnapTimeCodesToFrames.UseFixedFrameRate = true;
            config.SnapTimeCodesToFrames.FixedFrameRate = fixedFrameRate;

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
    public async Task Convert_SnapActive_RoundsEveryTimeCodeToNearestFrame()
    {
        var result = await RunConvertAsync(snapActive: true);

        Assert.Equal(3, result.Paragraphs.Count);

        Assert.Equal(1000, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(4000, result.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(4000, result.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(6480, result.Paragraphs[1].EndTime.TotalMilliseconds); // 6499 ms is 162.475 frames
    }

    [Fact]
    public async Task Convert_SnapActive_KeepsSubFrameCueOneFrameLong()
    {
        var result = await RunConvertAsync(snapActive: true);

        // Both ends round to 10,000 - the cue would otherwise end up empty or inverted.
        var p = result.Paragraphs[2];
        Assert.Equal(10000, p.StartTime.TotalMilliseconds);
        Assert.Equal(10040, p.EndTime.TotalMilliseconds);
    }

    [Fact]
    public async Task Convert_SnapNotActive_LeavesTimeCodesAlone()
    {
        var result = await RunConvertAsync(snapActive: false);

        Assert.Equal(1013, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3987, result.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(10009, result.Paragraphs[2].EndTime.TotalMilliseconds);
    }
}
