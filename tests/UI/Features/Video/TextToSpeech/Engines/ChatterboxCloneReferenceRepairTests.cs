using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// A reference voice that is not 24 kHz mono is re-encoded in place before synthesis, because the
/// voices folder is documented and users copy WAVs into it by hand (#13508). That check runs per
/// line, so a repair that cannot succeed - no ffmpeg on the machine, a WAV ffmpeg will not decode -
/// has to be attempted once, not once for every line of the subtitle.
/// </summary>
public class ChatterboxCloneReferenceRepairTests
{
    /// <summary>A canonical 44-byte PCM WAV header followed by <paramref name="sampleBytes"/> of silence.</summary>
    private static byte[] Wav(int sampleRate, short channels, short bitsPerSample, int sampleBytes)
    {
        var blockAlign = (short)(channels * bitsPerSample / 8);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + sampleBytes);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);                                   // PCM fmt chunk size
        writer.Write((short)1);                             // WAVE_FORMAT_PCM
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * blockAlign);              // byte rate
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(sampleBytes);
        writer.Write(new byte[sampleBytes]);
        writer.Flush();

        return stream.ToArray();
    }

    private static string WriteTempFile(byte[] content, string extension = ".wav")
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        File.WriteAllBytes(fileName, content);
        return fileName;
    }

    [Fact]
    public void CloneReadyWav_IsAccepted()
    {
        var fileName = WriteTempFile(Wav(24000, channels: 1, bitsPerSample: 16, sampleBytes: 480));
        try
        {
            Assert.True(ChatterboxTtsCpp.IsCloneReadyReferenceWav(fileName));
            Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Theory]
    [InlineData(48000, 1, 16)] // wrong sample rate
    [InlineData(24000, 2, 16)] // stereo
    [InlineData(24000, 1, 8)]  // 8 bit
    public void WavTheBackendCannotCloneFrom_IsRejected(int sampleRate, short channels, short bitsPerSample)
    {
        var fileName = WriteTempFile(Wav(sampleRate, channels, bitsPerSample, sampleBytes: 480));
        try
        {
            Assert.False(ChatterboxTtsCpp.IsCloneReadyReferenceWav(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void NoReferenceAtAll_NeedsNothing()
    {
        // The baked default voice is not cloning.
        Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(null));
        Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(string.Empty));

        // A reference that has gone missing is the server's business, not a conversion failure.
        Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav")));
    }

    // The set of paths whose repair already failed. Read directly: whether a second call really
    // skipped the ffmpeg run cannot be timed reliably on a machine that has no ffmpeg to spawn.
    private static ICollection<string> FailedRepairs()
    {
        var field = typeof(ChatterboxTtsCpp).GetField(
            "FailedCloneReferenceRepairs",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        return ((ConcurrentDictionary<string, byte>)field.GetValue(null)!).Keys;
    }

    [Fact]
    public void ARepairThatCannotSucceed_IsOnlyAttemptedOnce()
    {
        // Not decodable by ffmpeg either, so the repair fails however the machine is set up.
        var fileName = WriteTempFile(Encoding.ASCII.GetBytes("this is not a wav file at all, not even close"));
        try
        {
            Assert.DoesNotContain(fileName, FailedRepairs());

            Assert.False(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName));
            Assert.Contains(fileName, FailedRepairs()); // remembered, so line two does not try again

            // Every following line of the subtitle comes back here. Without the guard each one
            // spawns another doomed ffmpeg run; with it the answer is a dictionary lookup.
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 20; i++)
            {
                Assert.False(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName));
            }

            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"20 repeat checks took {stopwatch.ElapsedMilliseconds} ms - the repair is being retried");
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    // A voice that never needed repairing must not end up on the "already failed" list, or a
    // later genuine repair of the same path would be skipped.
    [Fact]
    public void AWorkingReference_IsNeverRecordedAsFailed()
    {
        var fileName = WriteTempFile(Wav(24000, channels: 1, bitsPerSample: 16, sampleBytes: 480));
        try
        {
            Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName));
            Assert.DoesNotContain(fileName, FailedRepairs());
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
