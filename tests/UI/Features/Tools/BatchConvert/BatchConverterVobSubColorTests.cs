using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Issue #15942: batch text → VobSub built the 4-color palette from a hard-coded white text /
/// black outline instead of the export-images profile, so a red (or blue, green...) font was
/// quantized to the black outline color and every subtitle came out black.
/// </summary>
public class BatchConverterVobSubColorTests
{
    private const string InputSrt = "1\n00:00:01,000 --> 00:00:03,000\nRed text.\n";

    [Fact]
    public async Task VobSub_PaletteUsesProfileFontAndOutlineColor()
    {
        var exportImages = Se.Settings.File.ExportImages;
        var oldProfiles = exportImages.Profiles;
        var oldLastProfileName = exportImages.LastProfileName;
        var dir = Directory.CreateTempSubdirectory("se-batch-vobsub-color-test");
        try
        {
            exportImages.Profiles =
            [
                new SeExportImagesProfile
                {
                    ProfileName = "Red",
                    FontColor = "#ffff0000",
                    OutlineColor = "#ff0000ff",
                },
            ];
            exportImages.LastProfileName = "Red";

            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                TargetFormatName = BatchConverter.FormatVobSub,
                SaveInSourceFolder = true,
                Overwrite = true,
            });

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), inputFile);
            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var idxFile = Path.Combine(dir.FullName, "movie.idx");
            Assert.True(File.Exists(idxFile), $"no .idx written, status: {item.Status}");
            var paletteLine = File.ReadAllLines(idxFile).Single(l => l.StartsWith("palette:", StringComparison.Ordinal));
            var palette = paletteLine["palette:".Length..].Split(',').Select(c => c.Trim()).ToArray();

            // Index 1 = pattern (text), index 2 = emphasis 1 (outline) - see VobSubWriter.WriteColors.
            Assert.Equal("ff0000", palette[1]);
            Assert.Equal("0000ff", palette[2]);
        }
        finally
        {
            exportImages.Profiles = oldProfiles;
            exportImages.LastProfileName = oldLastProfileName;
            dir.Delete(recursive: true);
        }
    }
}
