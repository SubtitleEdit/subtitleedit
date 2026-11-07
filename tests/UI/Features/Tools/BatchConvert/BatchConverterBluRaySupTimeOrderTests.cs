using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert to Blu-ray sup with lines out of time order - an ASSA script grouped by what the
/// author worked on (issue #16714). The file has to follow the timeline: a decoder drops what
/// comes after the timeline has passed it.
/// </summary>
public class BatchConverterBluRaySupTimeOrderTests
{
    [Fact]
    public async Task BluRaySup_UnsortedOverlappingLines_PtsNeverGoBackwards()
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-sup-order-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("{\\an8}Breaking news", 10000, 20000));
            subtitle.Paragraphs.Add(new Paragraph("First line", 11000, 13000));
            subtitle.Paragraphs.Add(new Paragraph("Second line", 14000, 16000));
            subtitle.Paragraphs.Add(new Paragraph("Before the sign", 2000, 4000));
            await File.WriteAllTextAsync(inputFile, new SubRip().ToText(subtitle, "movie"), TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                TargetFormatName = BatchConverter.FormatBluRaySup,
                SaveInSourceFolder = true,
                Overwrite = true,
            });

            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var supFile = Path.Combine(dir.FullName, "movie.sup");
            Assert.True(File.Exists(supFile), $"no .sup written, status: {item.Status}");

            var sup = await File.ReadAllBytesAsync(supFile, TestContext.Current.CancellationToken);
            var pts = new List<long>();
            var objectCounts = new List<int>();
            var position = 0;
            while (position + 13 <= sup.Length)
            {
                Assert.Equal((byte)'P', sup[position]);
                Assert.Equal((byte)'G', sup[position + 1]);
                pts.Add(((long)sup[position + 2] << 24) | ((long)sup[position + 3] << 16) | ((long)sup[position + 4] << 8) | sup[position + 5]);
                if (sup[position + 10] == 0x16)
                {
                    objectCounts.Add(sup[position + 13 + 10]);
                }

                position += 13 + (sup[position + 11] << 8 | sup[position + 12]);
            }

            Assert.Equal(pts.OrderBy(p => p), pts);
            Assert.Contains(2, objectCounts);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
