using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using System;
using System.IO;
using System.Linq;

namespace LibSETests.ContainerFormats;

/// <summary>
/// The parallel cluster walk (used for files on network shares, #13609) must produce
/// exactly the same subtitles as the sequential walk, for every subtitle track.
/// </summary>
public class MatroskaFileParallelTest
{
    [Theory]
    [InlineData("sample_MKV_SRT.mkv")]
    [InlineData("sample_MKV_VobSub_PGS.mkv")]
    public void ParallelClusterReadMatchesSequential(string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);

        using var sequential = new MatroskaFile(path);
        Assert.True(sequential.IsValid);
        var tracks = sequential.GetTracks(subtitleOnly: true);
        Assert.NotEmpty(tracks);

        MatroskaFile.ForceParallelClusterRead = true;
        try
        {
            using var parallel = new MatroskaFile(path);
            Assert.True(parallel.IsValid);
            var parallelTracks = parallel.GetTracks(subtitleOnly: true);
            Assert.Equal(tracks.Select(t => t.TrackNumber), parallelTracks.Select(t => t.TrackNumber));

            foreach (var track in tracks)
            {
                var expected = sequential.GetSubtitle(track.TrackNumber, null);
                var actual = parallel.GetSubtitle(track.TrackNumber, null);

                Assert.NotEmpty(expected);
                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Start, actual[i].Start);
                    Assert.Equal(expected[i].Duration, actual[i].Duration);
                    Assert.Equal(expected[i].GetData(track), actual[i].GetData(track));
                }
            }
        }
        finally
        {
            MatroskaFile.ForceParallelClusterRead = false;
        }
    }
}
