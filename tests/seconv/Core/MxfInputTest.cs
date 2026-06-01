using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Verifies ContainerSubtitleLoader's new MXF branch. There's no real-world MXF fixture
/// in the repo, so each test builds a minimal valid MXF byte stream from scratch: a
/// Header Partition Pack signature + a single Essence Element KLV whose payload is SRT
/// text. The libse MxfParser only needs the header signature + KLV walking to succeed —
/// any extra MXF structural metadata is optional.
/// </summary>
public class MxfInputTest : IDisposable
{
    private readonly string _tempRoot;

    public MxfInputTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Mxf_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ConvertAsync_MxfWithSrtEssence_ProducesSrtOutput()
    {
        var mxfPath = Path.Combine(_tempRoot, "movie.mxf");
        var srt =
            "1\n00:00:01,000 --> 00:00:04,000\nHello from MXF!\n\n"
            + "2\n00:00:05,000 --> 00:00:08,000\nSecond subtitle.\n\n";
        File.WriteAllBytes(mxfPath, BuildSyntheticMxf([srt]));

        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [mxfPath],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var srts = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Single(srts);
        // The MXF loader tags the essence with a "mxf_track{n}" suffix so multi-essence
        // files don't collide on output. Verify the suffix made it into the filename.
        Assert.Contains(srts, p => Path.GetFileName(p).Contains("mxf_track1"));

        var content = await File.ReadAllTextAsync(srts[0], TestContext.Current.CancellationToken);
        Assert.Contains("Hello from MXF!", content);
        Assert.Contains("Second subtitle.", content);
    }

    [Fact]
    public async Task ConvertAsync_MxfWithTrackNumberFilter_PicksOneEssence()
    {
        // Two-essence MXF, request only essence #2 via --track-number. The first
        // essence's text must not appear in the output.
        var mxfPath = Path.Combine(_tempRoot, "movie.mxf");
        var srt1 = "1\n00:00:01,000 --> 00:00:02,000\nFirst essence.\n\n";
        var srt2 = "1\n00:00:03,000 --> 00:00:04,000\nSecond essence.\n\n";
        File.WriteAllBytes(mxfPath, BuildSyntheticMxf([srt1, srt2]));

        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [mxfPath],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
            TrackNumbers = [2],
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var srts = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Single(srts);
        Assert.Contains(srts, p => Path.GetFileName(p).Contains("mxf_track2"));
        var content = await File.ReadAllTextAsync(srts[0], TestContext.Current.CancellationToken);
        Assert.Contains("Second essence.", content);
        Assert.DoesNotContain("First essence.", content);
    }

    [Fact]
    public async Task ConvertAsync_MxfWithCorruptKlv_ProducesMxfContextualError()
    {
        // A file with a valid Header Partition Pack signature but a truncated KLV
        // packet should surface a "Failed to parse MXF" error, not a raw
        // IndexOutOfRangeException leaked from libse's KLV reader.
        var mxfPath = Path.Combine(_tempRoot, "corrupt.mxf");
        File.WriteAllBytes(mxfPath, BuildCorruptMxf());
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [mxfPath],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
        });

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Contains("MXF", result.Errors[0]);
    }

    [Fact]
    public async Task ConvertAsync_MxfWithoutValidHeader_FallsThrough()
    {
        // A .mxf file without the Header Partition Pack signature should be treated as
        // a regular file (MxfParser.IsValid = false → LoadMxf returns null → caller
        // falls back to LibSEIntegration.LoadSubtitleWithFormat). With non-MXF bytes in
        // the .mxf file, that fallback will fail with a parse error — but we only need
        // to verify the MXF path didn't claim the file.
        var mxfPath = Path.Combine(_tempRoot, "fake.mxf");
        File.WriteAllText(mxfPath, "Not really an MXF file, just text with a .mxf extension.");
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [mxfPath],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
        });

        // Fallback should fail because the file isn't a parseable subtitle either — but
        // the error must come from the text loader, not from MXF assertions.
        Assert.False(result.Success);
        Assert.Single(result.Errors);
        // The error message shouldn't mention MXF — it should be the generic format
        // detection failure.
        Assert.DoesNotContain("MXF", result.Errors[0]);
    }

    /// <summary>
    /// Builds a minimal valid MXF byte stream: 16-byte Header Partition Pack key with
    /// no payload, followed by one Essence Element KLV per entry in <paramref name="payloads"/>
    /// carrying the entry as UTF-8 bytes. This is the smallest structure MxfParser will
    /// walk successfully — header signature at offset 0, then N KLV essences containing
    /// the subtitle text(s).
    /// </summary>
    private static byte[] BuildSyntheticMxf(IReadOnlyList<string> payloads)
    {
        // Header Partition Pack: only bytes 0..10 are checked by ReadHeaderPartitionPack;
        // the remaining 5 bytes can be anything per the spec's wildcard convention.
        byte[] headerKey = [0x06, 0x0E, 0x2B, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00];
        // Essence Element key: last 4 bytes wildcarded in KlvPacket.IsKey, so any 0xff
        // placeholder works.
        byte[] essenceKey = [0x06, 0x0E, 0x2B, 0x34, 0x01, 0x02, 0x01, 0x01, 0x0D, 0x01, 0x03, 0x01, 0xFF, 0xFF, 0xFF, 0xFF];

        using var ms = new MemoryStream();
        // First KLV: header partition pack with zero-length payload (BER 0x00).
        ms.Write(headerKey);
        ms.WriteByte(0x00);
        // Each payload becomes its own essence-element KLV. BER long-form length:
        // 0x84 means "4-byte big-endian length follows", which handles payloads up to
        // 4 GB and avoids the short-form 0..127 boundary case.
        foreach (var payload in payloads)
        {
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            ms.Write(essenceKey);
            ms.WriteByte(0x84);
            ms.WriteByte((byte)((payloadBytes.Length >> 24) & 0xFF));
            ms.WriteByte((byte)((payloadBytes.Length >> 16) & 0xFF));
            ms.WriteByte((byte)((payloadBytes.Length >> 8) & 0xFF));
            ms.WriteByte((byte)(payloadBytes.Length & 0xFF));
            ms.Write(payloadBytes);
        }
        // ReadHeaderPartitionPack requires the file to be >= 100 bytes; pad if smaller.
        // (With typical multi-cue SRT payload, we're already well past 100 bytes, but
        // the guard keeps short single-cue tests honest.)
        while (ms.Length < 100)
        {
            ms.WriteByte(0x00);
        }
        return ms.ToArray();
    }

    /// <summary>
    /// Builds a byte stream with a valid Header Partition Pack signature followed by
    /// a KLV that announces a huge payload but is truncated before the data — exercises
    /// the error path where the libse KLV reader runs off the end of the file.
    /// </summary>
    private static byte[] BuildCorruptMxf()
    {
        byte[] headerKey = [0x06, 0x0E, 0x2B, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x01, 0x00, 0x00, 0x00];
        byte[] essenceKey = [0x06, 0x0E, 0x2B, 0x34, 0x01, 0x02, 0x01, 0x01, 0x0D, 0x01, 0x03, 0x01, 0xFF, 0xFF, 0xFF, 0xFF];

        using var ms = new MemoryStream();
        ms.Write(headerKey);
        ms.WriteByte(0x00);
        ms.Write(essenceKey);
        // Claim a 1 MB payload but write only a few bytes — the parser's stream.Read
        // will return a short count and downstream parsing trips up.
        ms.WriteByte(0x83);
        ms.WriteByte(0x0F); ms.WriteByte(0xFF); ms.WriteByte(0xFF);
        ms.Write([0x01, 0x02, 0x03]);
        while (ms.Length < 100)
        {
            ms.WriteByte(0x00);
        }
        return ms.ToArray();
    }
}
