using SeConv.Core;
using SkiaSharp;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// PSP UMD Video (.MPS): one output per subtitle stream, named umd1, umd2, ...
/// </summary>
public class UmdVideoInputTest : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), "UmdVideo_" + Guid.NewGuid().ToString("N"));

    public UmdVideoInputTest()
    {
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private static readonly byte[] PackHeader = { 0, 0, 1, 0xBA, 0x44, 0, 4, 0, 4, 1, 1, 0x89, 0xC3, 0xF8 };

    /// <summary>A pack with one PES holding a whole subtitle record of the given sub-stream.</summary>
    private static byte[] MakeSubtitlePack(int subStreamId, long pts, uint durationTicks)
    {
        using var bitmap = new SKBitmap(40, 12);
        bitmap.Erase(SKColors.White);
        using var png = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        var header = Encoding.ASCII.GetBytes("0088").Concat(BigEndian(durationTicks)).Concat(new byte[] { 0, 0x10, 0, 100, 1, 100, 0, 0 });
        var body = header.Concat(png.ToArray()).ToArray();

        var pes = new List<byte> { 0, 0, 1, 0xBD, 0, 0, 0x81, 0x80, 5 };
        pes.AddRange(new[]
        {
            (byte)(0x21 | ((pts >> 29) & 0x0E)), (byte)(pts >> 22), (byte)(0x01 | ((pts >> 14) & 0xFE)), (byte)(pts >> 7), (byte)(0x01 | ((pts << 1) & 0xFE)),
        });
        pes.AddRange(new byte[] { (byte)subStreamId, 0 });
        pes.AddRange(BigEndian((uint)body.Length));
        pes.AddRange(body);
        pes[4] = (byte)((pes.Count - 6) >> 8);
        pes[5] = (byte)(pes.Count - 6);
        return PackHeader.Concat(pes).ToArray();
    }

    private static byte[] BigEndian(uint value) => new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

    [Fact]
    public async Task ConvertAsync_UmdVideo_WritesOneFilePerSubtitleStream()
    {
        var input = Path.Combine(_tempRoot, "00048.MPS");
        File.WriteAllBytes(input, MakeSubtitlePack(0x80, 90_000, 180_000)
            .Concat(MakeSubtitlePack(0x81, 270_000, 90_000))
            .Concat(MakeSubtitlePack(0x80, 450_000, 90_000))
            .ToArray());
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
            TimeCodesOnly = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var first = await File.ReadAllTextAsync(Path.Combine(outputFolder, "00048.umd1.srt"), TestContext.Current.CancellationToken);
        var second = await File.ReadAllTextAsync(Path.Combine(outputFolder, "00048.umd2.srt"), TestContext.Current.CancellationToken);
        Assert.Contains("00:00:01,000 --> 00:00:03,000", first);
        Assert.Contains("00:00:05,000 --> 00:00:06,000", first);
        Assert.Contains("00:00:03,000 --> 00:00:04,000", second);
    }
}
