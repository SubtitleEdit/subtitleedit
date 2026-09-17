using Nikse.SubtitleEdit.UiLogic.Media;

namespace LibUiLogicTests.Media;

/// <summary>
/// The loop finder behind Video &gt; Generate background music. Real input is ACE-Step output; these
/// use a synthetic song with the same traits: a steady beat, a four-bar chord phrase that repeats,
/// and an outro that fades to silence.
/// </summary>
public class MusicLooperTests : IDisposable
{
    private const int SampleRate = 16000;
    private readonly string _folder;

    public MusicLooperTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-music-looper-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }
    }

    /// <summary>Kick on every beat, a chord per bar cycling C-Am-F-G, the last four bars fading out.</summary>
    private static MusicAudio MakeSong(double bpm, double seconds, int channels = 2, double gain = 0.5)
    {
        var chords = new[]
        {
            new[] { 261.63, 329.63, 392.00 },
            new[] { 220.00, 261.63, 329.63 },
            new[] { 174.61, 220.00, 261.63 },
            new[] { 196.00, 246.94, 293.66 },
        };

        var frames = (int)(seconds * SampleRate);
        var samples = new float[frames * channels];
        var beat = 60.0 / bpm;
        var bar = beat * 4;
        var fadeStart = seconds - 4 * bar;
        for (var i = 0; i < frames; i++)
        {
            var t = (double)i / SampleRate;
            var beatPos = t % beat;
            var kick = Math.Sin(2 * Math.PI * 70 * beatPos) * Math.Exp(-beatPos * 30);
            var chord = chords[(int)(t / bar) % 4];
            var barPos = t % bar;
            var pad = chord.Sum(f => Math.Sin(2 * Math.PI * f * t)) / 3 * (0.6 + 0.4 * Math.Exp(-barPos * 3));
            var value = (0.6 * kick + 0.4 * pad) * gain;
            if (t > fadeStart)
            {
                value *= Math.Max(0, 1 - (t - fadeStart) / (seconds - fadeStart));
            }

            for (var c = 0; c < channels; c++)
            {
                samples[i * channels + c] = (float)value;
            }
        }

        return new MusicAudio(SampleRate, channels, samples);
    }

    [Fact]
    public void FindLoop_SteadySong_IsBarAlignedWholePhrasesBeforeTheOutro()
    {
        var song = MakeSong(120, 60);

        var loop = MusicLooper.FindLoop(song, 120);

        Assert.True(loop.IsBarAligned);
        Assert.InRange(loop.MeasuredBpm, 119.5, 120.5);
        Assert.Equal(0, loop.Bars % 4);
        Assert.True(loop.Bars >= 8);
        Assert.InRange(loop.SeamScore, 0.9, 1.0);
        Assert.InRange(loop.CrossfadeSeconds, 1.95, 2.05);
        // Never into the fade: the last four bars (52-60 s) are the outro.
        Assert.True(loop.EndSeconds <= 52.1, $"loop ends at {loop.EndSeconds:0.00} s, inside the outro");
        Assert.True(loop.StartSeconds >= loop.CrossfadeSeconds, "the crossfade needs pre-roll before the loop start");
    }

    [Fact]
    public void FindLoop_TempoOffFromRequest_MeasuresTheRealTempo()
    {
        // ACE-Step was asked for 110 bpm and delivered 113; a fixed 110 grid drifts a second off.
        var song = MakeSong(113, 60);

        var loop = MusicLooper.FindLoop(song, 110);

        Assert.True(loop.IsBarAligned);
        Assert.InRange(loop.MeasuredBpm, 112.5, 113.5);
        Assert.InRange(loop.SeamScore, 0.9, 1.0);
    }

    [Fact]
    public void FindLoop_TooShortForAGrid_FallsBackToPlainCrossfade()
    {
        var song = MakeSong(120, 6);

        var loop = MusicLooper.FindLoop(song, 120);

        Assert.False(loop.IsBarAligned);
        Assert.True(loop.EndSeconds > loop.StartSeconds);
        Assert.True(loop.StartSeconds >= loop.CrossfadeSeconds);
    }

    [Fact]
    public void Render_LongerThanSource_HasExactLengthKeepsLevelAndFadesOut()
    {
        var song = MakeSong(120, 60);
        var loop = MusicLooper.FindLoop(song, 120);

        var rendered = MusicLooper.Render(song, loop, 185.5, fadeOutSeconds: 3);

        Assert.Equal((int)Math.Round(185.5 * SampleRate), rendered.FrameCount);
        Assert.Equal(song.Channels, rendered.Channels);

        var mono = rendered.ToMono();
        double Rms(double from, double to)
        {
            var a = (int)(from * SampleRate);
            var b = (int)(to * SampleRate);
            return Math.Sqrt(mono.Skip(a).Take(b - a).Sum(v => (double)v * v) / (b - a));
        }

        // Whole phrases (8 s) across the body, including the loop wraps, stay at one level - no dips
        // into silence where the outro would have been.
        var reference = Rms(8, 16);
        for (var start = 16.0; start + 8 <= 176; start += 8)
        {
            Assert.InRange(Rms(start, start + 8), reference * 0.8, reference * 1.25);
        }

        Assert.True(Math.Abs(mono[^1]) < 1e-3, "the end is faded out");
    }

    [Fact]
    public void Render_ShorterThanLoopEnd_IsATrim()
    {
        var song = MakeSong(120, 60);
        var loop = MusicLooper.FindLoop(song, 120);

        var rendered = MusicLooper.Render(song, loop, 10, fadeOutSeconds: 0);

        Assert.Equal(10 * SampleRate, rendered.FrameCount);
        Assert.Equal(song.Samples.Take(rendered.Samples.Length), rendered.Samples);
    }

    [Fact]
    public void Normalize_HotInput_IsCappedAtThePeakCeiling()
    {
        // ACE-Step decodes up to +3.8 dBFS.
        var song = MakeSong(120, 20, gain: 1.6);
        Assert.True(song.GetPeak() > 1.0f);

        var normalized = MusicLooper.Normalize(song, targetRmsDb: -10, peakCeilingDb: -1);

        Assert.InRange(20 * Math.Log10(normalized.GetPeak()), -1.01, -0.99);
    }

    [Fact]
    public void Normalize_QuietInput_ReachesTargetRms()
    {
        var song = MakeSong(120, 20, gain: 0.05);

        var normalized = MusicLooper.Normalize(song, targetRmsDb: -20, peakCeilingDb: -1);

        var mono = normalized.ToMono();
        var blocks = mono.Chunk(SampleRate / 10).Where(b => b.Length == SampleRate / 10)
            .Select(b => b.Sum(v => (double)v * v) / b.Length).Where(p => p > 1e-6).ToList();
        var rmsDb = 10 * Math.Log10(blocks.Average());
        Assert.InRange(rmsDb, -20.5, -19.5);
    }

    [Fact]
    public void WritePcm16Wav_ReadWav_RoundTrips()
    {
        var song = MakeSong(120, 2);
        var fileName = Path.Combine(_folder, "roundtrip.wav");

        song.WritePcm16Wav(fileName);
        var read = MusicAudio.ReadWav(fileName);

        Assert.Equal(song.SampleRate, read.SampleRate);
        Assert.Equal(song.Channels, read.Channels);
        Assert.Equal(song.FrameCount, read.FrameCount);
        for (var i = 0; i < song.Samples.Length; i += 997)
        {
            Assert.InRange(read.Samples[i] - song.Samples[i], -1.0f / 16384, 1.0f / 16384);
        }
    }

    [Fact]
    public void ReadWav_Float32_KeepsSamplesAboveFullScale()
    {
        // audio.cpp --out-format float32: the whole point is that +3.8 dBFS survives.
        var fileName = Path.Combine(_folder, "float.wav");
        var samples = new[] { 0.5f, -0.25f, 1.55f, -1.2f };
        using (var writer = new BinaryWriter(File.Create(fileName)))
        {
            writer.Write("RIFF"u8.ToArray());
            writer.Write(36 + samples.Length * 4);
            writer.Write("WAVE"u8.ToArray());
            writer.Write("fmt "u8.ToArray());
            writer.Write(16);
            writer.Write((short)3); // IEEE float
            writer.Write((short)2);
            writer.Write(48000);
            writer.Write(48000 * 2 * 4);
            writer.Write((short)8);
            writer.Write((short)32);
            writer.Write("data"u8.ToArray());
            writer.Write(samples.Length * 4);
            foreach (var s in samples)
            {
                writer.Write(s);
            }
        }

        var read = MusicAudio.ReadWav(fileName);

        Assert.Equal(48000, read.SampleRate);
        Assert.Equal(2, read.Channels);
        Assert.Equal(samples, read.Samples);
        Assert.Equal(1.55f, read.GetPeak());
    }
}
