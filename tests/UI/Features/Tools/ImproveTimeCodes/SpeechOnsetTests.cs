using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

namespace UITests.Features.Tools.ImproveTimeCodes;

public class SpeechOnsetTests
{
    private const int Rate = 16000;

    /// <summary>Quiet noise with a loud tone in each given (start, end) range.</summary>
    private static float[] Make(double seconds, params (double Start, double End, double Amplitude)[] sounds)
    {
        var random = new Random(1);
        var samples = new float[(int)(seconds * Rate)];
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i / (double)Rate;
            var x = (random.NextDouble() - 0.5) * 0.004; // about -55 dB
            foreach (var (start, end, amplitude) in sounds)
            {
                if (t >= start && t < end)
                {
                    x += amplitude * Math.Sin(2 * Math.PI * 180 * t);
                }
            }

            samples[i] = (float)x;
        }

        return samples;
    }

    [Fact]
    public void LateStart_MovesBackToWhereTheSoundRises()
    {
        var levels = SpeechOnset.FromSamples(Make(4, (1.80, 3.0, 0.1)), Rate);

        var onset = SpeechOnset.Find(levels, 1.92, 0);

        Assert.NotNull(onset);
        Assert.InRange(onset!.Value, 1.77, 1.81);
    }

    [Fact]
    public void StartInQuiet_IsNeverMovedLater()
    {
        var levels = SpeechOnset.FromSamples(Make(4, (1.80, 3.0, 0.1)), Rate);

        Assert.Null(SpeechOnset.Find(levels, 1.72, 0));
    }

    [Fact]
    public void RunningSpeech_WithNoQuietBefore_IsLeftAlone()
    {
        var levels = SpeechOnset.FromSamples(Make(4, (0.5, 3.0, 0.1)), Rate);

        Assert.Null(SpeechOnset.Find(levels, 1.9, 0));
    }

    [Fact]
    public void NeverGoesBackPastTheSpeechBefore()
    {
        var levels = SpeechOnset.FromSamples(Make(4, (1.80, 3.0, 0.1)), Rate);

        Assert.Null(SpeechOnset.Find(levels, 1.92, 1.85));
    }

    [Fact]
    public void ShortDip_IsNotTakenForTheStart()
    {
        // A 10 ms break inside the word (a stop consonant) is passed over.
        var levels = SpeechOnset.FromSamples(Make(4, (1.70, 1.85, 0.1), (1.86, 3.0, 0.1)), Rate);

        var onset = SpeechOnset.Find(levels, 1.95, 0);

        Assert.NotNull(onset);
        Assert.InRange(onset!.Value, 1.67, 1.71);
    }

    [Fact]
    public void FlatAudio_SaysNothing()
    {
        var levels = SpeechOnset.FromSamples(Make(4), Rate);

        Assert.Null(SpeechOnset.Find(levels, 1.9, 0));
    }

    [Fact]
    public void ReadsAWaveFile()
    {
        var samples = Make(2, (1.0, 1.5, 0.1));
        var fileName = Path.Combine(Path.GetTempPath(), "se-onset-" + Guid.NewGuid() + ".wav");
        try
        {
            using (var writer = new BinaryWriter(File.Create(fileName)))
            {
                writer.Write("RIFF"u8.ToArray());
                writer.Write(36 + (samples.Length * 2));
                writer.Write("WAVEfmt "u8.ToArray());
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(Rate);
                writer.Write(Rate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write("data"u8.ToArray());
                writer.Write(samples.Length * 2);
                foreach (var s in samples)
                {
                    writer.Write((short)(s * 32767));
                }
            }

            var levels = SpeechOnset.FromWaveFile(fileName);

            Assert.NotNull(levels);
            Assert.Equal(200, levels!.Count);
            Assert.InRange(SpeechOnset.Find(levels, 1.1, 0)!.Value, 0.97, 1.01);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void NotAWaveFile_GivesNull()
    {
        Assert.Null(SpeechOnset.FromWaveFile(Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid() + ".wav")));
    }
}
