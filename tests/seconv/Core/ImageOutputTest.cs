using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class ImageOutputTest : IDisposable
{
    private readonly string _tempRoot;

    public ImageOutputTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "ImgOut_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello, World!

        2
        00:00:05,000 --> 00:00:08,000
        This is a test subtitle.

        """;

    private async Task<ConversionResult> ConvertTo(string format, string outFolderName)
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent);
        var outFolder = Path.Combine(_tempRoot, outFolderName);
        Directory.CreateDirectory(outFolder);

        var converter = new SubtitleConverter();
        return await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
            Resolution = (1920, 1080),
        });
    }

    [Fact]
    public async Task ConvertAsync_BluRaySupOutput_ProducesNonEmptyBinaryFile()
    {
        var result = await ConvertTo("bluraysup", "sup");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var supFiles = Directory.GetFiles(Path.Combine(_tempRoot, "sup"), "*.sup");
        Assert.Single(supFiles);
        Assert.True(new FileInfo(supFiles[0]).Length > 1000, "Blu-Ray sup file should be > 1 KB for 2 paragraphs");
    }

    [Fact]
    public async Task ConvertAsync_BdnXmlOutput_ProducesPngsAndIndex()
    {
        var result = await ConvertTo("bdnxml", "bdn");
        Assert.True(result.Success, string.Join("; ", result.Errors));

        // BDN-XML emits to a subfolder named after the input stem; recurse to find PNGs.
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "bdn"), "*.png", SearchOption.AllDirectories);
        Assert.Equal(2, pngs.Length);

        // Each PNG starts with the standard PNG signature
        foreach (var png in pngs)
        {
            var sig = new byte[8];
            using var fs = File.OpenRead(png);
            Assert.Equal(8, fs.Read(sig, 0, 8));
            Assert.Equal(0x89, sig[0]);
            Assert.Equal((byte)'P', sig[1]);
            Assert.Equal((byte)'N', sig[2]);
            Assert.Equal((byte)'G', sig[3]);
        }

        var indexes = Directory.GetFiles(Path.Combine(_tempRoot, "bdn"), "index.xml", SearchOption.AllDirectories);
        Assert.Single(indexes);
        var index = await File.ReadAllTextAsync(indexes[0]);
        Assert.Contains("<BDN", index);
        Assert.Contains("<Event", index);
        Assert.Contains("0001.png", index);
        Assert.Contains("0002.png", index);
    }

    [Fact]
    public async Task ConvertAsync_FcpImageOutput_Succeeds()
    {
        var result = await ConvertTo("fcpimage", "fcp");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "fcp"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }

    [Fact]
    public async Task ConvertAsync_DostOutput_Succeeds()
    {
        var result = await ConvertTo("dost", "dost");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "dost"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }

    [Fact]
    public async Task ConvertAsync_DCinemaInteropOutput_Succeeds()
    {
        var result = await ConvertTo("dcinemainterop", "dci");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "dci"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }

    [Fact]
    public async Task ConvertAsync_ImagesWithTimeCodeOutput_Succeeds()
    {
        var result = await ConvertTo("imageswithtimecode", "tc");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "tc"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }
}
