using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using System.IO;

namespace LibSETests.ContainerFormats;

public class MatroskaClosedCaptionTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    /// <summary>
    /// The TS closed caption samples (ffmpeg fate-suite Closedcaption_rollup.m2v with "-a53cc 1")
    /// stream-copied to Matroska: the captions travel inside the H.264 (SEI) and MPEG-2 (user data)
    /// video and read back the same as from the transport stream.
    /// </summary>
    [Theory]
    [InlineData("sample_mkv_cea608_h264.mkv")]
    [InlineData("sample_mkv_cea608_mpeg2.mkv")]
    public void ReadsCea608AndCea708FromVideoTrack(string fileName)
    {
        using var matroska = new MatroskaFile(FilePath(fileName));

        var tracks = MatroskaClosedCaptionReader.Read(matroska, MatroskaClosedCaptionReader.DefaultProbeMilliseconds, null);

        Assert.Equal(new[] { 1, 101 }, tracks.Keys);
        var cc1 = tracks[1];
        Assert.Equal("(<i>inaudible radio chatter</i>)", cc1[0].Text);
        Assert.InRange(cc1[0].StartTime.TotalMilliseconds, 966, 970);
        Assert.Equal("(<i>inaudible radio chatter</i>)" + System.Environment.NewLine + ">> Safety remains our number one", cc1[1].Text);

        var cea708 = tracks[101];
        Assert.Equal(">> Safety remains our number one", cea708[1].Text);
    }

    /// <summary>
    /// A video without captions is not read to the end - reading stops after the probe time.
    /// </summary>
    [Fact]
    public void StopsReadingVideoWithoutCaptionsAfterProbeTime()
    {
        using var matroska = new MatroskaFile(FilePath("sample_mkv_no_captions_4min.mkv"));
        long lastPosition = 0;
        long total = 0;

        var tracks = MatroskaClosedCaptionReader.Read(matroska, 60_000, (position, length) =>
        {
            lastPosition = position;
            total = length;
        });

        Assert.Empty(tracks);
        Assert.True(lastPosition < total / 2, $"read {lastPosition} of {total} bytes");
    }

    /// <summary>
    /// A corrupt video block (its size is smaller than its own header) must not throw - reading
    /// returns what could be decoded.
    /// </summary>
    [Fact]
    public void CorruptVideoBlockDoesNotThrow()
    {
        var data = File.ReadAllBytes(FilePath("sample_mkv_cea608_mpeg2.mkv"));
        var cluster = FindBytes(data, new byte[] { 0x1F, 0x43, 0xB6, 0x75 }, 0);
        var block = FindBytes(data, new byte[] { 0xA3, 0x41, 0x29 }, cluster); // first SimpleBlock, 297 bytes
        Assert.True(cluster > 0 && block > cluster);
        data[block + 1] = 0x40;
        data[block + 2] = 0x02; // 2 bytes - less than track number + timecode + flags

        var fileName = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(fileName, data);
            using var matroska = new MatroskaFile(fileName);

            var tracks = MatroskaClosedCaptionReader.Read(matroska, MatroskaClosedCaptionReader.DefaultProbeMilliseconds, null);

            Assert.NotNull(tracks);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    private static int FindBytes(byte[] data, byte[] pattern, int start)
    {
        for (var i = start; i <= data.Length - pattern.Length; i++)
        {
            if (data.AsSpan(i, pattern.Length).SequenceEqual(pattern))
            {
                return i;
            }
        }

        return -1;
    }
}
