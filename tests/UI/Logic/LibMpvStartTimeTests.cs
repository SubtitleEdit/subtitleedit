using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic;

/// <summary>
/// mpv counts a transport stream from the file's start ("rebase-start-time=yes") - as the
/// subtitles read from it are timed - and every other file on its own time stamps (#9828).
/// </summary>
public sealed class LibMpvStartTimeTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "se-mpv-start-" + Guid.NewGuid().ToString("N"));

    public LibMpvStartTimeTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, true);
    }

    private string WritePackets(string name, int packetSize, int syncOffset)
    {
        var bytes = new byte[packetSize * 40];
        for (var i = syncOffset; i < bytes.Length; i += packetSize)
        {
            bytes[i] = 0x47;
        }

        var fileName = Path.Combine(_folder, name);
        File.WriteAllBytes(fileName, bytes);
        return fileName;
    }

    [Fact]
    public void TransportStream_CountsFromTheFileStart()
    {
        Assert.True(LibMpvDynamicPlayer.UseFileStartAsZero(WritePackets("a.ts", 188, 0)));
    }

    [Fact]
    public void BluRayM2ts_CountsFromTheFileStart()
    {
        Assert.True(LibMpvDynamicPlayer.UseFileStartAsZero(WritePackets("a.m2ts", 192, 4)));
    }

    [Fact]
    public void Mp4_KeepsItsOwnTimeStamps()
    {
        var fileName = Path.Combine(_folder, "a.mp4");
        var bytes = new byte[8000];
        new byte[] { 0, 0, 0, 0x18, (byte)'f', (byte)'t', (byte)'y', (byte)'p' }.CopyTo(bytes, 0);
        File.WriteAllBytes(fileName, bytes);

        Assert.False(LibMpvDynamicPlayer.UseFileStartAsZero(fileName));
    }

    [Theory]
    [InlineData("https://example.com/live.ts")]
    [InlineData("")]
    public void UrlsAndMissingFiles_KeepTheirOwnTimeStamps(string path)
    {
        Assert.False(LibMpvDynamicPlayer.UseFileStartAsZero(path));
        Assert.False(LibMpvDynamicPlayer.UseFileStartAsZero(Path.Combine(_folder, "missing.ts")));
    }
}
