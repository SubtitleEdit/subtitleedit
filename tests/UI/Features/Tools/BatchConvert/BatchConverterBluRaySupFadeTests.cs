using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch text → Blu-ray sup rebuilt the image parameters without the fade keyframes parsed from
/// "{\fad(..)}", so the fade the main window's export writes (palette update display sets) was
/// silently dropped.
/// </summary>
public class BatchConverterBluRaySupFadeTests
{
    private const byte PresentationCompositionSegment = 0x16;

    private static async Task<int> CountCompositionSegmentsAsync(string text)
    {
        var srt = "1\n00:00:01,000 --> 00:00:03,000\n" + text + "\n";
        var dir = Directory.CreateTempSubdirectory("se-batch-sup-fade-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            await File.WriteAllTextAsync(inputFile, srt, TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                TargetFormatName = BatchConverter.FormatBluRaySup,
                SaveInSourceFolder = true,
                Overwrite = true,
            });

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, srt.SplitToLines(), inputFile);
            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var supFile = Path.Combine(dir.FullName, "movie.sup");
            Assert.True(File.Exists(supFile), $"no .sup written, status: {item.Status}");

            // "PG" + PTS(4) + DTS(4) + type(1) + size(2) + payload
            var sup = await File.ReadAllBytesAsync(supFile, TestContext.Current.CancellationToken);
            var count = 0;
            var position = 0;
            while (position + 13 <= sup.Length)
            {
                Assert.Equal((byte)'P', sup[position]);
                Assert.Equal((byte)'G', sup[position + 1]);
                if (sup[position + 10] == PresentationCompositionSegment)
                {
                    count++;
                }

                position += 13 + (sup[position + 11] << 8 | sup[position + 12]);
            }

            return count;
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task BluRaySup_FadeTagWritesPaletteUpdates()
    {
        var plain = await CountCompositionSegmentsAsync("Fade me.");
        var faded = await CountCompositionSegmentsAsync("{\\fad(500,500)}Fade me.");

        // Plain: show + clear. Faded: the same plus a palette update per fade step.
        Assert.Equal(2, plain);
        Assert.True(faded > plain, $"expected fade palette updates, got {faded} composition segments");
    }
}
