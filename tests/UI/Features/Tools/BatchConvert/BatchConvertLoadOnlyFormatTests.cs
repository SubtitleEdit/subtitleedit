using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System.Text;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert did not know the load-only text formats (SubtitleFormat.GetTextOtherFormats):
/// a WSB, FTE, JSON "load only", ... file was added as "Unknown" and could not be converted,
/// although File > Open reads it.
/// </summary>
public class BatchConvertLoadOnlyFormatTests
{
    private const string WordsJson =
        "{\"words\":[{\"time\":1.0,\"duration\":0.5,\"name\":\"Hello\"},{\"time\":1.5,\"duration\":0.2,\"name\":\".\"}," +
        "{\"time\":3.0,\"duration\":0.6,\"name\":\"World\"}]}";

    public BatchConvertLoadOnlyFormatTests()
    {
        // The binary-format probes before the fallback use code pages that Program registers.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static List<BatchConvertItem> AddFile(string fileName) => BatchConvertViewModel.AddFile(fileName);

    [AvaloniaFact]
    public async Task LoadOnlyJsonFile_IsAddedWithItsFormat_AndConverts()
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-load-only-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "words.json");
            await File.WriteAllTextAsync(inputFile, WordsJson, TestContext.Current.CancellationToken);

            var item = Assert.Single(AddFile(inputFile));
            Assert.Equal(new JsonTypeOnlyLoad1().Name, item.Format);
            Assert.NotNull(item.Subtitle);
            Assert.Equal(2, item.Subtitle.Paragraphs.Count);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                SaveInSourceFolder = true,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            });
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var outputFile = Path.Combine(dir.FullName, "words.srt");
            Assert.True(File.Exists(outputFile), "no .srt written");
            var converted = Subtitle.Parse(outputFile);
            Assert.NotNull(converted);
            Assert.Equal(2, converted.Paragraphs.Count);
            Assert.Equal("Hello.", converted.Paragraphs[0].Text);
            Assert.Equal("World", converted.Paragraphs[1].Text);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [AvaloniaFact]
    public void UnrecognizedFile_IsStillUnknown()
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-load-only-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "prose.txt");
            File.WriteAllText(inputFile, "Just some prose." + Environment.NewLine + "Nothing timed here.");

            var item = Assert.Single(AddFile(inputFile));
            Assert.Null(item.Subtitle);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
