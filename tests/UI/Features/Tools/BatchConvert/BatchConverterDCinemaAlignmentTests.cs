using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch text → D-Cinema PNG rebuilt the image parameters with a bottom-center alignment. The
/// D-Cinema handlers place the image only by its alignment (Valign/Halign), so "{\an8}" lines
/// that the main window's export puts at the top came out at the bottom.
/// </summary>
public class BatchConverterDCinemaAlignmentTests
{
    private const string InputSrt =
        "1\n00:00:01,000 --> 00:00:03,000\n{\\an8}Top line.\n\n" +
        "2\n00:00:04,000 --> 00:00:06,000\n{\\an4}Middle left.\n\n" +
        "3\n00:00:07,000 --> 00:00:09,000\nBottom line.\n";

    [Theory]
    [InlineData(BatchConverter.FormatDCinemaInterop)]
    [InlineData(BatchConverter.FormatDCinemaSmpte2014)]
    public async Task DCinema_KeepsAlignmentTags(string targetFormat)
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-dcinema-align-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                TargetFormatName = targetFormat,
                SaveInSourceFolder = true,
                Overwrite = true,
            });

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), inputFile);
            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var xmlFile = Directory.GetFiles(dir.FullName, "*.xml", SearchOption.AllDirectories).SingleOrDefault();
            Assert.True(xmlFile != null, $"no .xml written, status: {item.Status}");
            var xml = (await File.ReadAllTextAsync(xmlFile, TestContext.Current.CancellationToken)).ToLowerInvariant();

            // Interop writes VAlign/HAlign, SMPTE Valign/Halign - compare case-insensitively.
            var images = xml.Split("<image ").Skip(1).ToArray();
            Assert.Equal(3, images.Length);
            Assert.Contains("valign=\"top\"", images[0]);
            Assert.Contains("valign=\"center\"", images[1]);
            Assert.Contains("halign=\"left\"", images[1]);
            Assert.Contains("valign=\"bottom\"", images[2]);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
