using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Voice seeding is best-effort and runs once per install, which is exactly why its failure
/// modes are expensive: whatever it leaves in the voices folder is what the engine clones from
/// for the rest of that install's life, and nothing retries it.
///
/// These tests do not need ffmpeg to be present - a source ffmpeg cannot decode and a missing
/// ffmpeg both land on the same failure path, which is the one being pinned.
/// </summary>
public class VoiceSeedHelperTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "se-voice-seed-tests-" + Guid.NewGuid().ToString("N"));

    public VoiceSeedHelperTests() => Directory.CreateDirectory(_folder);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // Temp folder cleanup is not what these tests are about.
        }
    }

    private string Path_(string name) => Path.Combine(_folder, name);

    [Fact]
    public void ExistingDestination_IsLeftAlone()
    {
        var src = Path_("src.wav");
        var dest = Path_("dest.wav");
        File.WriteAllText(src, "new");
        File.WriteAllText(dest, "already seeded");

        VoiceSeedHelper.CopyOrResample(src, dest, 24000, "Test");

        Assert.Equal("already seeded", File.ReadAllText(dest));
    }

    /// <summary>
    /// A failed conversion must still leave a usable voice behind. This is where the old code
    /// went wrong: it guarded the fallback with "if the destination does not exist", so a
    /// conversion that failed *after* opening its output (a kill, a timeout, a partial write)
    /// left the truncated file in place and the copy never ran.
    /// </summary>
    [Fact]
    public void FailedConversion_FallsBackToAVerbatimCopy()
    {
        var src = Path_("src.wav");
        var dest = Path_("dest.wav");
        File.WriteAllText(src, "not actually a wav, so ffmpeg cannot convert it");

        VoiceSeedHelper.CopyOrResample(src, dest, 24000, "Test");

        Assert.True(File.Exists(dest));
        Assert.Equal(File.ReadAllBytes(src), File.ReadAllBytes(dest));
    }

    /// <summary>
    /// Qwen3's Base backend rejects a reference at any rate but 24 kHz, so for it a failed
    /// resample means "skip this voice" - and skipping must not leave the failed output behind
    /// either, or the engine seeds a voice the server refuses.
    /// </summary>
    [Fact]
    public void FailedConversion_WithoutCopyFallback_LeavesNothingBehind()
    {
        var src = Path_("src.wav");
        var dest = Path_("dest.wav");
        File.WriteAllText(src, "not actually a wav, so ffmpeg cannot convert it");

        VoiceSeedHelper.CopyOrResample(src, dest, 24000, "Test", copyOnFailure: false);

        Assert.False(File.Exists(dest));
    }

    [Fact]
    public void RealWav_IsSeeded()
    {
        var src = Path_("src.wav");
        var dest = Path_("dest.wav");
        File.WriteAllBytes(src, MakeSilentWav(sampleRate: 8000, milliseconds: 50));

        VoiceSeedHelper.CopyOrResample(src, dest, 24000, "Test");

        // Resampled when ffmpeg is on the box, plain-copied when it is not - either way the
        // voice has to end up in the folder as a non-empty file.
        Assert.True(File.Exists(dest));
        Assert.True(new FileInfo(dest).Length > 0);
    }

    private static byte[] MakeSilentWav(int sampleRate, int milliseconds)
    {
        var samples = sampleRate * milliseconds / 1000;
        var dataBytes = samples * 2; // mono, 16-bit

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataBytes);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);              // PCM header size
        writer.Write((short)1);        // PCM
        writer.Write((short)1);        // mono
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2);  // byte rate
        writer.Write((short)2);        // block align
        writer.Write((short)16);       // bits per sample
        writer.Write("data"u8.ToArray());
        writer.Write(dataBytes);
        writer.Write(new byte[dataBytes]);
        writer.Flush();

        return stream.ToArray();
    }
}
