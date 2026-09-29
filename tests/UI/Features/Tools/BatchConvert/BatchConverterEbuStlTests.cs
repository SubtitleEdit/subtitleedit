using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// EBU STL is a binary format that also sits in the text format list (so it can be loaded),
/// where its ToText only returns "Not supported!". Batch convert must write it through the
/// binary save, not the text loop - otherwise the ".stl" file is that 14-byte string. The
/// binary save also needs <see cref="Ebu.EbuUiHelper"/>, which used to be set only when the
/// EBU settings dialog had been opened; without it the file was left empty.
/// </summary>
public class BatchConverterEbuStlTests
{
    public BatchConverterEbuStlTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private const string InputSrt = @"1
00:00:01,000 --> 00:00:03,000
Hello world.

2
00:00:04,000 --> 00:00:06,000
Second line.
";

    [Fact]
    public async Task SrtToEbuStl_WritesBinaryStlFile()
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-ebu-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                SaveInSourceFolder = true,
                Overwrite = true,
                TargetFormatName = BatchConverter.FormatEbuStl,
            });

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), inputFile);
            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var outputFile = Path.Combine(dir.FullName, "movie.stl");
            Assert.True(File.Exists(outputFile), "no .stl written");
            var bytes = await File.ReadAllBytesAsync(outputFile, TestContext.Current.CancellationToken);
            Assert.True(bytes.Length >= 1024 + 2 * 128, $"expected GSI + TTI blocks, got {bytes.Length} bytes");

            var reloaded = new Subtitle();
            var ebu = new Ebu();
            Assert.True(ebu.IsMine(null, outputFile), "output is not recognized as EBU STL");
            ebu.LoadSubtitle(reloaded, null, outputFile);
            Assert.Equal(2, reloaded.Paragraphs.Count);
            Assert.Equal("Hello world.", reloaded.Paragraphs[0].Text);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    /// <summary>
    /// DVB Teletext is binary too but not in the batch converter's own binary list - after the
    /// text loop learned to skip binary formats, it fell through to the image writer and failed.
    /// </summary>
    [Fact]
    public async Task SrtToDvbTeletext_WritesBinaryFile()
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-dvbttx-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig
            {
                SaveInSourceFolder = true,
                Overwrite = true,
                TargetFormatName = DvbTeletext.NameOfFormat,
            });

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), inputFile);
            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var outputFile = Path.Combine(dir.FullName, "movie.dvbttx");
            Assert.True(File.Exists(outputFile), "no .dvbttx written");

            var reloaded = new Subtitle();
            var format = new DvbTeletext();
            Assert.True(format.IsMine(null!, outputFile), "output is not recognized as DVB Teletext");
            format.LoadSubtitle(reloaded, null!, outputFile);
            Assert.Equal(2, reloaded.Paragraphs.Count);
            Assert.Equal("Hello world.", reloaded.Paragraphs[0].Text);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
