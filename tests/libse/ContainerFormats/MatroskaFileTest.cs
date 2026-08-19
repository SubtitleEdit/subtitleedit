using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using System.IO;

namespace LibSETests.ContainerFormats;

/// <summary>
/// Guards the cluster walk that extracts subtitles from a Matroska file. The walk is tuned for
/// I/O (buffer sizes differ for local disk and network shares, see #6772/#13609), and this test
/// exists so such tuning cannot quietly change what is parsed out of the file.
/// </summary>
public class MatroskaFileTest
{
    [Theory]
    [InlineData("sample_MKV_SRT.mkv")]
    [InlineData("sample_MKV_VobSub_PGS.mkv")]
    public void ClusterWalkExtractsSubtitles(string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);

        using var matroska = new MatroskaFile(path);
        Assert.True(matroska.IsValid);

        var tracks = matroska.GetTracks(subtitleOnly: true);
        Assert.NotEmpty(tracks);

        foreach (var track in tracks)
        {
            var subtitles = matroska.GetSubtitle(track.TrackNumber, null);
            Assert.NotEmpty(subtitles);

            var previousStart = long.MinValue;
            foreach (var subtitle in subtitles)
            {
                Assert.True(subtitle.Start >= previousStart, "blocks must come out in playback order");
                Assert.True(subtitle.Duration >= 0);
                Assert.NotEmpty(subtitle.GetData(track));
                previousStart = subtitle.Start;
            }
        }
    }

    /// <summary>
    /// ffmpeg writes an all-ones BlockDuration - its "unknown duration" marker - on the image
    /// subtitle tracks it muxes. Read as a signed tick count that is negative, so every block
    /// used to come out ending one millisecond before it started, and the VobSub OCR window
    /// showed nothing but negative durations. An unusable duration has to read as "unknown".
    /// </summary>
    [Fact]
    public void UnknownBlockDurationIsNotNegative()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_ffmpeg_VobSub_no_duration.mkv");

        using var matroska = new MatroskaFile(path);
        Assert.True(matroska.IsValid);

        var tracks = matroska.GetTracks(subtitleOnly: true);
        Assert.NotEmpty(tracks);

        foreach (var track in tracks)
        {
            var subtitles = matroska.GetSubtitle(track.TrackNumber, null);
            Assert.NotEmpty(subtitles);
            Assert.All(subtitles, s => Assert.True(s.End >= s.Start, "a block cannot end before it starts"));
        }
    }

    /// <summary>
    /// A file truncated mid-cluster (partial download, recording in progress) still declares the
    /// full segment size in its header, so the cluster walk's end position lies beyond EOF. The
    /// walk used to spin forever at EOF (id reads yield None, Seek(0) makes no progress) instead
    /// of returning what it had - opening such a file hung SE at 100% CPU.
    /// </summary>
    [Fact]
    public void TruncatedFileTerminates()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_SRT.mkv");
        var bytes = File.ReadAllBytes(path);
        var tempFileName = Path.GetTempFileName();
        try
        {
            // cut mid-file so the segment (and likely a cluster) extends past EOF
            using (var fs = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(bytes, 0, bytes.Length * 6 / 10);
            }

            using var matroska = new MatroskaFile(tempFileName);
            Assert.True(matroska.IsValid);

            foreach (var track in matroska.GetTracks(subtitleOnly: true))
            {
                matroska.GetSubtitle(track.TrackNumber, null); // must terminate
            }
        }
        finally
        {
            File.Delete(tempFileName);
        }
    }
}
