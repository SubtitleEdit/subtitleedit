using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

/// <summary>
/// An HLS web rip is an MPEG transport stream, but it is often saved (or renamed) as .mp4. Opening
/// one ran only the MP4 parser, found nothing and ended at the "open as video?" prompt, so the
/// CEA-608/708 captions in its video stream were never read. The open path now sniffs the content.
/// </summary>
public class TransportStreamUnderOtherExtensionTests : IDisposable
{
    private readonly string _directory;

    public TransportStreamUnderOtherExtensionTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.TransportStreamUnderOtherExtensionTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(".mp4")]
    [InlineData(".MP4")]
    [InlineData(".m4v")]
    [InlineData(".mov")]
    [InlineData(".mkv")]
    public void TransportStreamWithVideoExtension_IsDetected(string ext)
    {
        var fileName = WriteTransportStream("rip" + ext);

        Assert.True(MainViewModel.IsTransportStreamUnderOtherVideoExtension(fileName, ext));
    }

    [Theory]
    [InlineData(".ts")]
    [InlineData(".m2ts")]
    [InlineData(".mts")]
    [InlineData(".mpg")]
    public void TransportStreamExtensions_AreLeftToTheirOwnHandlers(string ext)
    {
        var fileName = WriteTransportStream("rip" + ext);

        Assert.False(MainViewModel.IsTransportStreamUnderOtherVideoExtension(fileName, ext));
    }

    [Fact]
    public void TransportStreamWithSubtitleExtension_IsNotDetected()
    {
        var fileName = WriteTransportStream("rip.srt");

        Assert.False(MainViewModel.IsTransportStreamUnderOtherVideoExtension(fileName, ".srt"));
    }

    [Fact]
    public void RealMp4_IsNotDetected()
    {
        var fileName = Path.Combine(_directory, "movie.mp4");
        var bytes = new byte[188 * 30];
        // ftyp box: size 24, "ftyp", brand "isom"
        bytes[3] = 24;
        "ftypisom"u8.CopyTo(bytes.AsSpan(4));
        File.WriteAllBytes(fileName, bytes);

        Assert.False(MainViewModel.IsTransportStreamUnderOtherVideoExtension(fileName, ".mp4"));
    }

    private string WriteTransportStream(string name)
    {
        var fileName = Path.Combine(_directory, name);
        var bytes = new byte[188 * 30];
        for (var i = 0; i < bytes.Length; i += 188)
        {
            bytes[i] = 0x47;
            bytes[i + 1] = 0x1F; // null packet PID 0x1FFF
            bytes[i + 2] = 0xFF;
            bytes[i + 3] = 0x10;
        }

        File.WriteAllBytes(fileName, bytes);
        return fileName;
    }
}
