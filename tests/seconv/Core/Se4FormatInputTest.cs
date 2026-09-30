using SeConv.Core;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// The SE 4 input formats the GUI opens again (#15449, #15453, #15456 and the image-list OCR
/// import): ARIB STD-B36, Adobe Premiere projects, and image-list files, which seconv used to
/// fail on or convert to their png file names.
/// </summary>
public class Se4FormatInputTest : IDisposable
{
    private readonly string _tempRoot;

    public Se4FormatInputTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _tempRoot = Path.Combine(Path.GetTempPath(), "Se4Input_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello, World!

        2
        00:00:05,000 --> 00:00:08,000
        This is a test subtitle.

        """;

    private async Task<(ConversionResult Result, string Output)> ToSubRip(string input, bool timeCodesOnly = false, string? ocrDb = null)
    {
        var outFolder = Path.Combine(_tempRoot, "out_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outFolder);
        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "subrip",
            OutputFolder = outFolder,
            Overwrite = true,
            TimeCodesOnly = timeCodesOnly,
            OcrEngine = ocrDb is null ? "tesseract" : "nocr",
            OcrDb = ocrDb,
            // The CLI turns isolation off for nOCR, which splits letters on the alpha channel.
            PgsIsolateColors = ocrDb is null,
        });

        var srt = Directory.GetFiles(outFolder, "*.srt").SingleOrDefault();
        return (result, srt is null ? string.Empty : await File.ReadAllTextAsync(srt, TestContext.Current.CancellationToken));
    }

    /// <summary>Writes <see cref="SrtContent"/> as image-based <paramref name="format"/> and returns the index file.</summary>
    private async Task<string> RenderImageList(string format, string pattern)
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);
        var outFolder = Path.Combine(_tempRoot, format);
        Directory.CreateDirectory(outFolder);
        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
            Resolution = (1920, 1080),
            ImageStyle = new ImageExportStyle(),
        });
        Assert.True(result.Success, string.Join("; ", result.Errors));
        return Directory.GetFiles(outFolder, pattern, SearchOption.AllDirectories).Single();
    }

    [Theory]
    [InlineData("BdnXml", "*.xml")]
    [InlineData("Dost", "*.dost")]
    [InlineData("FcpImage", "*.xml")]
    [InlineData("ImscImage", "*.ttml")]
    public async Task ImageListInput_TimeCodesOnly_KeepsTimingNotFileNames(string format, string pattern)
    {
        var index = await RenderImageList(format, pattern);

        var (result, srt) = await ToSubRip(index, timeCodesOnly: true);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Contains("00:00:01,000 --> 00:00:04,000", srt);
        Assert.Contains("00:00:05,000 --> 00:00:08,000", srt);
        Assert.DoesNotContain(".png", srt);
    }

    [Theory]
    [InlineData("BdnXml", "*.xml")]
    [InlineData("ImscImage", "*.ttml")]
    public async Task ImageListInput_IsOcrd(string format, string pattern)
    {
        var ocrDb = FindRepoFile(Path.Combine("Ocr", "Latin.nocr"));
        Assert.SkipWhen(ocrDb is null, "Ocr/Latin.nocr not found");
        var index = await RenderImageList(format, pattern);

        var (result, srt) = await ToSubRip(index, ocrDb: ocrDb);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Contains("Hello, World!", srt);
        Assert.Contains("This is a test subtitle.", srt);
    }

    [Fact]
    public async Task SubRipWithImageFileNames_TimeCodesOnly_DropsTheFileNames()
    {
        var sb = new StringBuilder();
        for (var i = 1; i <= 4; i++)
        {
            sb.AppendLine(i.ToString());
            sb.AppendLine($"00:00:0{i * 2 - 1},000 --> 00:00:0{i * 2},000");
            sb.AppendLine($"{i:0000}.png");
            sb.AppendLine();
        }

        var input = Path.Combine(_tempRoot, "images.srt");
        await File.WriteAllTextAsync(input, sb.ToString(), TestContext.Current.CancellationToken);

        var (result, srt) = await ToSubRip(input, timeCodesOnly: true);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Contains("00:00:07,000 --> 00:00:08,000", srt);
        Assert.DoesNotContain(".png", srt);
    }

    [Fact]
    public async Task OrdinarySubRipMentioningAnImageStaysText()
    {
        var input = Path.Combine(_tempRoot, "text.srt");
        await File.WriteAllTextAsync(input, SrtContent + "3\r\n00:00:09,000 --> 00:00:10,000\r\nSave it as logo.png\r\n", TestContext.Current.CancellationToken);

        var (result, srt) = await ToSubRip(input);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Contains("Save it as logo.png", srt);
    }

    [Fact]
    public async Task AribB36Input_ConvertsItsCaptions()
    {
        var input = Path.Combine(_tempRoot, "captions.2hd");
        await File.WriteAllBytesAsync(input, MakeAribB36(("000001000", "000003000", "Hello"), ("000004000", "000006000", "World")), TestContext.Current.CancellationToken);

        var (result, srt) = await ToSubRip(input);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Contains("00:00:01,000 --> 00:00:03,000", srt);
        Assert.Contains("Hello", srt);
        Assert.Contains("World", srt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PremiereProjectInput_ConvertsItsTextClips(bool gzipped)
    {
        var bytes = Encoding.UTF8.GetBytes(ProjectXml);
        if (gzipped)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }

            bytes = output.ToArray();
        }

        var input = Path.Combine(_tempRoot, "project.prproj");
        await File.WriteAllBytesAsync(input, bytes, TestContext.Current.CancellationToken);

        var (result, srt) = await ToSubRip(input);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Contains("00:00:04,000 --> 00:00:06,000", srt);
        Assert.Contains("Hello title", srt);
    }

    private static string? FindRepoFile(string relativePath)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    // One "Text" clip from 4 to 6 seconds - Premiere counts 254016000000 ticks per second.
    private const string ProjectXml = """
        <PremiereData Version="3">
          <VideoComponentChain ObjectID="10">
            <ComponentChain>
              <Components>
                <Component ObjectRef="20"/>
              </Components>
            </ComponentChain>
          </VideoComponentChain>
          <VideoClipTrackItem ObjectID="30">
            <ClipTrackItem>
              <ComponentOwner>
                <Components ObjectRef="10"/>
              </ComponentOwner>
              <TrackItem>
                <Start>1016064000000</Start>
                <End>1524096000000</End>
              </TrackItem>
            </ClipTrackItem>
          </VideoClipTrackItem>
          <VideoFilterComponent ObjectID="20">
            <Component>
              <DisplayName>Text</DisplayName>
              <InstanceName>Hello title</InstanceName>
            </Component>
          </VideoFilterComponent>
        </PremiereData>
        """;

    /// <summary>Builds an ARIB B36 file with one page per (start, end, alphanumeric text) cue.</summary>
    private static byte[] MakeAribB36(params (string Start, string End, string Text)[] cues)
    {
        const int blockSize = 256;
        var buffer = new byte[Math.Max(3072, blockSize * (3 + cues.Length) + blockSize)];
        Encoding.ASCII.GetBytes("DCAPTION").CopyTo(buffer, 0);

        for (var i = 0; i < cues.Length; i++)
        {
            var block = blockSize * (3 + i);
            buffer[block + 2] = 0;
            buffer[block + 3] = 250; // block length
            buffer[block + 4] = 0x2a; // page management information

            var index = block + 5;
            const int pageManagementLength = 120;
            buffer[index++] = 0;
            buffer[index++] = pageManagementLength;
            Encoding.ASCII.GetBytes("T").CopyTo(buffer, index + 9); // timing unit: milliseconds
            Encoding.ASCII.GetBytes(cues[i].Start).CopyTo(buffer, index + 10); // HHMMSSmmm
            Encoding.ASCII.GetBytes(cues[i].End).CopyTo(buffer, index + 19);
            index += pageManagementLength;

            buffer[index++] = 0x3a; // caption text page management data
            const int captionTextPageManagementLength = 20;
            buffer[index++] = 0;
            buffer[index++] = captionTextPageManagementLength;
            Encoding.ASCII.GetBytes("eng").CopyTo(buffer, index + 14);
            index += captionTextPageManagementLength;

            buffer[index++] = 0x4a; // caption text data
            var textData = new List<byte> { 0x9b }; // CSI
            textData.AddRange(Encoding.ASCII.GetBytes("170;30 a"));
            textData.Add(0x0e); // LS1: alphanumeric set into GL
            textData.Add(0x89); // MSZ: middle size, so the letters decode half width
            textData.AddRange(Encoding.ASCII.GetBytes(cues[i].Text));
            var unitLength = 5 + textData.Count;
            buffer[index++] = 0;
            buffer[index++] = (byte)(15 + unitLength);
            buffer[index + 14] = (byte)unitLength; // data unit loop length
            var unit = index + 15;
            buffer[unit++] = 0x1f; // unit separator
            buffer[unit++] = 0x20; // statement body (text)
            buffer[unit++] = 0;
            buffer[unit++] = 0;
            buffer[unit++] = (byte)textData.Count;
            textData.ToArray().CopyTo(buffer, unit);
        }

        return buffer;
    }
}
