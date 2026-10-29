using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// "Change frame rate" only rescaled the milliseconds; frame-based writers kept writing frames
/// and their frame rate from libse's current frame rate - the source's - so an ITT saved after
/// 25 -> 23.976 still said ttp:frameRate="25" (#16257).
/// </summary>
public class BatchConverterChangeFrameRateTests : IDisposable
{
    private readonly double _currentFrameRate = Configuration.Settings.General.CurrentFrameRate;
    private readonly string _itunesTemplate = Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate;

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _currentFrameRate;
        Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate = _itunesTemplate;
    }

    private const string InputSrt = @"1
00:00:01,000 --> 00:00:03,000
Hello world.
";

    [Fact]
    public async Task ChangeFrameRate_ItunesTimedTextUsesTargetFrameRate()
    {
        Configuration.Settings.General.CurrentFrameRate = 25;
        Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate = string.Empty;
        var dir = Directory.CreateTempSubdirectory("se-batch-itt-fps-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                SaveInSourceFolder = true,
                Overwrite = true,
                TargetFormatName = new ItunesTimedText().Name,
                ChangeFrameRate = new BatchConvertConfig.ChangeFrameRateSettings
                {
                    IsActive = true,
                    FromFrameRate = 25,
                    ToFrameRate = 23.976,
                },
            });

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), inputFile);
            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var raw = await File.ReadAllTextAsync(Path.Combine(dir.FullName, "movie.itt"), TestContext.Current.CancellationToken);
            Assert.Contains("ttp:frameRate=\"24\"", raw);
            Assert.Contains("ttp:frameRateMultiplier=\"999 1000\"", raw);

            // the main window's frame rate is left alone
            Assert.Equal(25, Configuration.Settings.General.CurrentFrameRate);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
