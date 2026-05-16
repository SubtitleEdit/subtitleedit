using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class Mp3DurationReaderTests
{
    // MPEG-1 Layer III, 128 kbps, 44.1 kHz, no padding, stereo.
    // 1152 samples per frame at 44100 Hz = 1152 / 44100 s per frame.
    // Frame size = (1152 / 8 * 128000 / 44100) = 417 bytes.
    private const double FrameDurationSeconds = 1152.0 / 44100.0;
    private const int FrameSize = 417;

    private static byte[] BuildMp3WithFrames(int frameCount)
    {
        var data = new byte[frameCount * FrameSize];
        for (var f = 0; f < frameCount; f++)
        {
            var offset = f * FrameSize;
            data[offset + 0] = 0xFF; // sync
            data[offset + 1] = 0xFB; // sync + MPEG 1 + Layer III + protection bit
            data[offset + 2] = 0x90; // bitrate index 9 (128 kbps), sample rate index 0 (44.1 kHz), no padding
            data[offset + 3] = 0x00; // stereo + no copyright + no original + no emphasis
        }

        return data;
    }

    [Fact]
    public void TryGetDurationSeconds_RawFrames_MatchesExpectedDuration()
    {
        var temp = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(temp, BuildMp3WithFrames(100));

            var result = Mp3DurationReader.TryGetDurationSeconds(temp);

            Assert.NotNull(result);
            Assert.Equal(100 * FrameDurationSeconds, result!.Value, precision: 5);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void TryGetDurationSeconds_WithId3v2Header_SkipsTagAndStillCounts()
    {
        var temp = Path.GetTempFileName();
        try
        {
            // 10-byte ID3v2 header declaring a 50-byte tag (synchsafe: 50 = 0x32),
            // followed by 50 zero bytes, followed by 50 MP3 frames.
            var id3 = new byte[10 + 50];
            id3[0] = (byte)'I';
            id3[1] = (byte)'D';
            id3[2] = (byte)'3';
            id3[3] = 0x04; // major version
            id3[4] = 0x00; // revision
            id3[5] = 0x00; // flags (no footer)
            id3[6] = 0x00;
            id3[7] = 0x00;
            id3[8] = 0x00;
            id3[9] = 0x32;

            var frames = BuildMp3WithFrames(50);
            var combined = new byte[id3.Length + frames.Length];
            Buffer.BlockCopy(id3, 0, combined, 0, id3.Length);
            Buffer.BlockCopy(frames, 0, combined, id3.Length, frames.Length);
            File.WriteAllBytes(temp, combined);

            var result = Mp3DurationReader.TryGetDurationSeconds(temp);

            Assert.NotNull(result);
            Assert.Equal(50 * FrameDurationSeconds, result!.Value, precision: 5);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void TryGetDurationSeconds_NonMp3File_ReturnsNull()
    {
        var temp = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(temp, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 });

            var result = Mp3DurationReader.TryGetDurationSeconds(temp);

            Assert.Null(result);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void TryGetDurationSeconds_MissingFile_ReturnsNull()
    {
        var result = Mp3DurationReader.TryGetDurationSeconds(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp3"));

        Assert.Null(result);
    }
}
