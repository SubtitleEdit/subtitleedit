using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using System;
using System.IO;
using System.Linq;

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

    /// <summary>
    /// A read may return fewer bytes than asked for without being at the end of the file - it
    /// happens on a busy network share (#14940). The fixed-length integer reads gave up on such
    /// a read and left the stream mid-element, so the rest of the file was parsed as garbage
    /// and no subtitles came out. Parsing must not depend on how the bytes are chunked.
    /// </summary>
    [Theory]
    [InlineData("sample_MKV_SRT.mkv")]
    [InlineData("sample_MKV_VobSub_PGS.mkv")]
    public void ShortReadsGiveSameSubtitles(string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);

        using var expected = new MatroskaFile(path);
        using var actual = new MatroskaFile(new ShortReadStream(new MemoryStream(File.ReadAllBytes(path))));
        Assert.True(actual.IsValid);

        var expectedTracks = expected.GetTracks(subtitleOnly: true);
        var actualTracks = actual.GetTracks(subtitleOnly: true);
        Assert.Equal(expectedTracks.Select(t => t.TrackNumber), actualTracks.Select(t => t.TrackNumber));

        foreach (var track in expectedTracks)
        {
            var expectedSubtitles = expected.GetSubtitle(track.TrackNumber, null);
            var actualSubtitles = actual.GetSubtitle(track.TrackNumber, null);
            Assert.NotEmpty(expectedSubtitles);
            Assert.Equal(expectedSubtitles.Select(s => (s.Start, s.Duration)), actualSubtitles.Select(s => (s.Start, s.Duration)));
        }
    }

    /// <summary>Hands out one byte per read, the shortest read a stream is allowed to make.</summary>
    private sealed class ShortReadStream : Stream
    {
        private readonly Stream _inner;

        public ShortReadStream(Stream inner) => _inner = inner;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, Math.Min(count, 1));
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
