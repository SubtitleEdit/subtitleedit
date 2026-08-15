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
}
