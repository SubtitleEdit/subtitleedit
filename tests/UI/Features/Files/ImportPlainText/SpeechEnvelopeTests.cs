using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Files.ImportPlainText;

public class SpeechEnvelopeTests
{
    /// <summary>Loud between the given seconds, near silent everywhere else.</summary>
    private static SpeechEnvelope Envelope(double totalSeconds, params (double From, double To)[] speech)
    {
        var rms = new double[(int)(totalSeconds / SpeechEnvelope.FrameSeconds)];
        for (var i = 0; i < rms.Length; i++)
        {
            var t = i * SpeechEnvelope.FrameSeconds;
            rms[i] = speech.Any(s => t >= s.From && t < s.To) ? 8000 : 20;
        }

        return new SpeechEnvelope(rms);
    }

    [Fact]
    public void End_IsWhereTheSpeechGoesQuiet()
    {
        var envelope = Envelope(20, (1.0, 2.6), (6.0, 8.0));

        var end = envelope.FindSpeechEnd(1.0, 5.0);

        Assert.NotNull(end);
        Assert.InRange(end.Value, 2.55, 2.65);
    }

    [Fact]
    public void ShortPause_InsideALine_IsNotTheEnd()
    {
        // 150 ms of quiet is a breath; the line carries on to 3.0.
        var envelope = Envelope(20, (1.0, 2.0), (2.15, 3.0));

        var end = envelope.FindSpeechEnd(1.0, 6.0);

        Assert.NotNull(end);
        Assert.InRange(end.Value, 2.95, 3.05);
    }

    [Fact]
    public void StartThatIsHalfASecondEarly_StillEndsWithTheSpeech()
    {
        // The aligner put the start at 1.0 but the line is only spoken from 1.6.
        var envelope = Envelope(20, (1.6, 3.2));

        var end = envelope.FindSpeechEnd(1.0, 6.0);

        Assert.NotNull(end);
        Assert.InRange(end.Value, 3.15, 3.25);
    }

    [Fact]
    public void LinePlacedInSilence_GivesNoEnd_SoReadingTimeStands()
    {
        // A closing line the aligner could only estimate: nothing is spoken anywhere near it.
        var envelope = Envelope(40, (1.0, 3.0));

        Assert.Null(envelope.FindSpeechEnd(20.0, 24.0));
    }

    [Fact]
    public void SpeechThatNeverStopsBeforeTheLimit_GivesNoEnd()
    {
        var envelope = Envelope(20, (1.0, 12.0));

        Assert.Null(envelope.FindSpeechEnd(1.0, 5.0));
    }

    [Fact]
    public void SilentRecording_GivesNoEnd()
    {
        var envelope = new SpeechEnvelope(new double[500]);

        Assert.Null(envelope.FindSpeechEnd(1.0, 5.0));
    }

    [Fact]
    public void WaveFile_IsReadWhateverItsSampleRateAndChannels()
    {
        // What the separator writes: 44.1 kHz stereo 16-bit. One second of tone, one of silence.
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        try
        {
            const int sampleRate = 44100;
            const int channels = 2;
            var data = new byte[sampleRate * 2 * channels * 2];
            for (var i = 0; i < sampleRate; i++)
            {
                var sample = (short)(Math.Sin(i * 2 * Math.PI * 220 / sampleRate) * 12000);
                for (var c = 0; c < channels; c++)
                {
                    var at = ((i * channels) + c) * 2;
                    data[at] = (byte)(sample & 0xff);
                    data[at + 1] = (byte)((sample >> 8) & 0xff);
                }
            }

            using (var writer = new BinaryWriter(File.Create(fileName)))
            {
                writer.Write("RIFF"u8);
                writer.Write(36 + data.Length);
                writer.Write("WAVEfmt "u8);
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2);
                writer.Write((short)(channels * 2));
                writer.Write((short)16);
                writer.Write("data"u8);
                writer.Write(data.Length);
                writer.Write(data);
            }

            var envelope = SpeechEnvelope.FromWaveFile(fileName);

            Assert.NotNull(envelope);
            var end = envelope.FindSpeechEnd(0.0, 2.0);
            Assert.NotNull(end);
            Assert.InRange(end.Value, 0.95, 1.05);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    private static List<SubtitleLineViewModel> ThreeLines() =>
    [
        new() { Text = "Hello there, how are you today?" },
        new() { Text = "Fine, thank you very much." },
        new() { Text = "And how is the family doing?" },
    ];

    // The second line is followed by six seconds of music before the third one starts.
    private static readonly List<(double Start, double End)> Aligned = [(1.0, 3.5), (3.5, 5.0), (12.0, 15.0)];

    private static void Apply(List<SubtitleLineViewModel> lines, SpeechEnvelope? envelope)
    {
        var texts = lines.Select(l => l.Text).ToList();
        ForcedAligner.ApplyTimeCodes(lines, Aligned, texts, [0, 1, 2], 20, envelope);
    }

    [Fact]
    public void Aligner_EndsTheLineWhereItsSpeechEnds()
    {
        var lines = ThreeLines();

        // The speech of line two runs longer than the aligner (or reading time) thought.
        Apply(lines, Envelope(20, (1.0, 3.3), (3.5, 6.2), (12.0, 14.1)));

        Assert.InRange(lines[1].StartTime.TotalSeconds, 3.45, 3.55);
        Assert.InRange(lines[1].EndTime.TotalSeconds, 6.15, 6.25);
        Assert.InRange(lines[2].EndTime.TotalSeconds, 14.05, 14.15);
    }

    [Fact]
    public void Aligner_WithoutAnEnvelope_IsUnchanged()
    {
        var withNull = ThreeLines();
        var silent = ThreeLines();

        Apply(withNull, null);
        Apply(silent, new SpeechEnvelope(new double[1000])); // nothing to go by: reading time stays

        Assert.Equal(withNull.Select(l => l.EndTime), silent.Select(l => l.EndTime));
    }

    [Fact]
    public void Aligner_SpeechEnd_NeverRunsIntoTheNextLine()
    {
        var lines = ThreeLines();

        // The "speech" carries on across the start of the second line.
        Apply(lines, Envelope(20, (1.0, 4.2), (12.0, 14.1)));

        Assert.True(lines[0].EndTime < lines[1].StartTime);
    }
}
