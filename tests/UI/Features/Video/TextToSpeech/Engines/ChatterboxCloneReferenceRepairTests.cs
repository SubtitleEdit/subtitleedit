using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using System.Collections;
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

    // The paths whose repair already failed. Read directly: whether a second call really skipped
    // the ffmpeg run cannot be timed reliably on a machine that has no ffmpeg to spawn. The value
    // type is the private FileStamp record, so only the keys are read here.
    private static ICollection<string> FailedRepairs()
    {
        var field = typeof(ChatterboxTtsCpp).GetField(
            "FailedCloneReferenceRepairs",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var dictionary = (IEnumerable)field.GetValue(null)!;

        var keys = new List<string>();
        foreach (var entry in dictionary)
        {
            keys.Add((string)entry.GetType().GetProperty("Key")!.GetValue(entry)!);
        }

        return keys;
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

    // The voices folder is documented and users copy WAVs into it by hand, so the obvious answer
    // to a rejected voice is to fix the file in place. A guard keyed on the path alone would go
    // on refusing the repaired file until SE restarts, so it is keyed on the contents that failed.
    [Fact]
    public void AReferenceRepairedInPlace_IsAcceptedWithoutARestart()
    {
        var fileName = WriteTempFile(Encoding.ASCII.GetBytes("this is not a wav file at all, not even close"));
        try
        {
            Assert.False(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName));
            Assert.Contains(fileName, FailedRepairs());

            // The user drops a proper 24 kHz mono WAV in over the broken one.
            File.WriteAllBytes(fileName, Wav(24000, channels: 1, bitsPerSample: 16, sampleBytes: 480));
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow.AddSeconds(1));

            Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName));
            Assert.DoesNotContain(fileName, FailedRepairs());
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    // ...and one that is replaced by a different but still unusable file is retried once more,
    // rather than being answered from the stale entry.
    [Fact]
    public void AReferenceReplacedByAnotherBrokenOne_IsJudgedOnItsOwnContents()
    {
        var fileName = WriteTempFile(Encoding.ASCII.GetBytes("not a wav"));
        try
        {
            Assert.False(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName));

            File.WriteAllBytes(fileName, Wav(48000, channels: 2, bitsPerSample: 16, sampleBytes: 960));
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow.AddSeconds(1));

            // Still not clone-ready, and with no ffmpeg the repair fails again - but it was the new
            // file that was judged, not the old entry.
            ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(fileName);

            var stamp = FailedRepairsStamp(fileName);
            if (stamp != null)
            {
                Assert.Equal(new FileInfo(fileName).Length, stamp.Value.Length);
            }
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    /// <summary>The recorded (ticks, length) for a path, or null when it is not recorded as failed.</summary>
    private static (long Ticks, long Length)? FailedRepairsStamp(string path)
    {
        var field = typeof(ChatterboxTtsCpp).GetField(
            "FailedCloneReferenceRepairs",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        foreach (var entry in (IEnumerable)field.GetValue(null)!)
        {
            var type = entry.GetType();
            var key = (string)type.GetProperty("Key")!.GetValue(entry)!;
            if (!string.Equals(key, path, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = type.GetProperty("Value")!.GetValue(entry)!;
            var valueType = value.GetType();
            return ((long)valueType.GetProperty("Ticks")!.GetValue(value)!,
                    (long)valueType.GetProperty("Length")!.GetValue(value)!);
        }

        return null;
    }
}
