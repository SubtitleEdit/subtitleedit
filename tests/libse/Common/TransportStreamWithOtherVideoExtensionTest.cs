using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;
using System.Text;

namespace LibSETests.Common;

/// <summary>
/// An HLS web rip is an MPEG transport stream, but it is often saved (or renamed) as .mp4. The main
/// window, batch convert and seconv routed it on the extension to the MP4 parser, which found
/// nothing, so the CEA-608/708 captions in its video stream were never read.
/// </summary>
public class TransportStreamWithOtherVideoExtensionTest : IDisposable
{
    private readonly string _directory;

    public TransportStreamWithOtherVideoExtensionTest()
    {
        _directory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.TransportStreamWithOtherVideoExtensionTest", Guid.NewGuid().ToString("N"));
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
    [InlineData("rip.mp4")]
    [InlineData("rip.MP4")]
    [InlineData("rip.m4v")]
    [InlineData("rip.mov")]
    [InlineData("rip.mkv")]
    public void TransportStreamWithVideoExtension_IsDetected(string name)
    {
        Assert.True(FileUtil.IsTransportStreamWithOtherVideoExtension(WriteTransportStream(name)));
    }

    [Theory]
    [InlineData("rip.ts")]
    [InlineData("rip.m2ts")]
    [InlineData("rip.mts")]
    [InlineData("rip.mpg")]
    public void TransportStreamExtensions_AreLeftToTheirOwnHandlers(string name)
    {
        Assert.False(FileUtil.IsTransportStreamWithOtherVideoExtension(WriteTransportStream(name)));
    }

    [Fact]
    public void TransportStreamWithSubtitleExtension_IsNotDetected()
    {
        Assert.False(FileUtil.IsTransportStreamWithOtherVideoExtension(WriteTransportStream("rip.srt")));
    }

    [Fact]
    public void RealMp4_IsNotDetected()
    {
        var fileName = Path.Combine(_directory, "movie.mp4");
        var bytes = new byte[188 * 60];
        bytes[3] = 24; // ftyp box: size 24, brand "isom"
        Encoding.ASCII.GetBytes("ftypisom").CopyTo(bytes, 4);
        File.WriteAllBytes(fileName, bytes);

        Assert.False(FileUtil.IsTransportStreamWithOtherVideoExtension(fileName));
    }

    [Fact]
    public void MissingFile_IsNotDetected()
    {
        Assert.False(FileUtil.IsTransportStreamWithOtherVideoExtension(Path.Combine(_directory, "missing.mp4")));
    }

    private string WriteTransportStream(string name)
    {
        var fileName = Path.Combine(_directory, name);
        var bytes = new byte[188 * 60];
        for (var i = 0; i < bytes.Length; i += 188)
        {
            bytes[i] = 0x47;
            bytes[i + 1] = 0x1F; // null packet, PID 0x1FFF
            bytes[i + 2] = 0xFF;
            bytes[i + 3] = 0x10;
        }

        File.WriteAllBytes(fileName, bytes);
        return fileName;
    }
}
