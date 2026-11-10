using System.Buffers.Binary;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace LibUiLogicTests.Media;

/// <summary>
/// The text to speech merge adds each line's audio into one silent track in a single pass
/// (#16767) instead of rewriting the full-length file per line through ffmpeg.
/// </summary>
public class PcmWaveMixerTests : IDisposable
{
    private readonly string _folder;

    public PcmWaveMixerTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-pcm-mixer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // ignored
        }
    }

    [Fact]
    public void EmptyTrack_IsValidSilentWave()
    {
        var fileName = Path.Combine(_folder, "silence.wav");
        using (var mixer = new PcmWaveMixer(fileName, 1000, 2, 1.5))
        {
            mixer.Complete();
        }

        var (sampleRate, channels, samples) = ReadWave(fileName);
        Assert.Equal(1000, sampleRate);
        Assert.Equal(2, channels);
        Assert.Equal(1500 * 2, samples.Length);
        Assert.All(samples, s => Assert.Equal(0, s));
    }

    [Fact]
    public void Clip_LandsAtStartFrame()
    {
        var fileName = Path.Combine(_folder, "one.wav");
        using (var mixer = new PcmWaveMixer(fileName, 100, 1, 1.0))
        {
            mixer.Mix(0.25, Pcm(1, 2, 3));
            mixer.Complete();
        }

        var samples = ReadWave(fileName).Samples;
        Assert.Equal(100, samples.Length);
        Assert.Equal(0, samples[24]);
        Assert.Equal(new short[] { 1, 2, 3 }, samples[25..28]);
        Assert.Equal(0, samples[28]);
    }

    [Fact]
    public void OverlappingClips_AreSummedWithClamping()
    {
        var fileName = Path.Combine(_folder, "overlap.wav");
        using (var mixer = new PcmWaveMixer(fileName, 100, 1, 1.0))
        {
            mixer.Mix(0.10, Pcm(30000, 10000, -30000, 5));
            mixer.Mix(0.11, Pcm(30000, -30000, 7));
            mixer.Complete();
        }

        var samples = ReadWave(fileName).Samples;
        Assert.Equal(new short[] { 30000, short.MaxValue, short.MinValue, 12 }, samples[10..14]);
    }

    [Fact]
    public void StereoClip_KeepsChannelOrder()
    {
        var fileName = Path.Combine(_folder, "stereo.wav");
        using (var mixer = new PcmWaveMixer(fileName, 10, 2, 1.0))
        {
            mixer.Mix(0.2, Pcm(1, -1, 2, -2));
            mixer.Complete();
        }

        var samples = ReadWave(fileName).Samples;
        Assert.Equal(new short[] { 0, 0, 1, -1, 2, -2, 0, 0 }, samples[2..10]);
    }

    [Fact]
    public void ClipPastEnd_ExtendsTrack()
    {
        var fileName = Path.Combine(_folder, "extend.wav");
        using (var mixer = new PcmWaveMixer(fileName, 10, 1, 1.0))
        {
            mixer.Mix(0.9, Pcm(1, 2, 3));
            mixer.Mix(2.0, Pcm(9));
            Assert.Equal(2.1, mixer.DurationSeconds, 6);
            mixer.Complete();
        }

        var samples = ReadWave(fileName).Samples;
        Assert.Equal(21, samples.Length);
        Assert.Equal(new short[] { 1, 2, 3 }, samples[9..12]);
        Assert.All(samples[12..20], s => Assert.Equal(0, s));
        Assert.Equal(9, samples[20]);
    }

    [Fact]
    public void NegativeStart_DropsClipHead()
    {
        var fileName = Path.Combine(_folder, "negative.wav");
        using (var mixer = new PcmWaveMixer(fileName, 10, 1, 1.0))
        {
            mixer.Mix(-0.2, Pcm(1, 2, 3, 4));
            mixer.Mix(-5, Pcm(7));
            mixer.Complete();
        }

        var samples = ReadWave(fileName).Samples;
        Assert.Equal(new short[] { 3, 4, 0 }, samples[0..3]);
    }

    [Fact]
    public void PartialTrailingFrame_IsIgnored()
    {
        var fileName = Path.Combine(_folder, "partial.wav");
        using (var mixer = new PcmWaveMixer(fileName, 10, 2, 1.0))
        {
            var pcm = Pcm(5, 6, 7); // 1.5 stereo frames
            mixer.Mix(0, pcm);
            mixer.Complete();
        }

        var samples = ReadWave(fileName).Samples;
        Assert.Equal(new short[] { 5, 6, 0, 0 }, samples[0..4]);
        Assert.Equal(20, samples.Length);
    }

    [Fact]
    public void DisposeWithoutComplete_ReleasesFile()
    {
        var fileName = Path.Combine(_folder, "abandoned.wav");
        using (var mixer = new PcmWaveMixer(fileName, 10, 1, 1.0))
        {
            mixer.Mix(0, Pcm(1));
        }

        File.Delete(fileName);
        Assert.False(File.Exists(fileName));
    }

    private static byte[] Pcm(params short[] samples)
    {
        var bytes = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), samples[i]);
        }

        return bytes;
    }

    private static (int SampleRate, int Channels, short[] Samples) ReadWave(string fileName)
    {
        var bytes = File.ReadAllBytes(fileName);
        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(bytes, 8, 4));
        Assert.Equal((uint)bytes.Length - 8, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4)));
        Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(20)));
        var channels = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(22));
        var sampleRate = (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24));
        Assert.Equal(16, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(34)));
        Assert.Equal("data", System.Text.Encoding.ASCII.GetString(bytes, 36, 4));
        var dataSize = (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(40));
        Assert.Equal(bytes.Length - 44, dataSize);

        var samples = new short[dataSize / 2];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(44 + i * 2));
        }

        return (sampleRate, channels, samples);
    }
}
