using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

public class CloneReferenceTailTests
{
    [Fact]
    public void PreparedFileName_IsInPreparedFolderNextToTheReference()
    {
        var reference = Path.Combine("voices", "Sophie_Anderson.wav");

        var prepared = CloneReferenceTail.GetPreparedFileName(reference);

        Assert.Equal(Path.Combine("voices", "prepared", "Sophie_Anderson.wav"), prepared);
    }

    [Fact]
    public void Stamp_ChangesWhenTheReferenceChanges_AndIsNullWhenMissing()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-clone-tail-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var reference = Path.Combine(folder, "voice.wav");
            File.WriteAllBytes(reference, new byte[100]);
            var first = CloneReferenceTail.BuildStamp(reference);
            Assert.NotNull(first);
            Assert.StartsWith($"v{CloneReferenceTail.RecipeVersion}|100|", first);

            File.WriteAllBytes(reference, new byte[200]);
            File.SetLastWriteTimeUtc(reference, DateTime.UtcNow.AddMinutes(1));
            Assert.NotEqual(first, CloneReferenceTail.BuildStamp(reference));

            Assert.Null(CloneReferenceTail.BuildStamp(Path.Combine(folder, "missing.wav")));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task PrepareAsync_MissingReference_ReturnsItUnchanged()
    {
        var missing = Path.Combine(Path.GetTempPath(), "se-clone-tail-missing-" + Guid.NewGuid().ToString("N") + ".wav");

        var result = await CloneReferenceTail.PrepareAsync(missing, "Test engine", CancellationToken.None);

        Assert.Equal(missing, result);
    }

    [Fact]
    public async Task PrepareAsync_ReusesACurrentPreparedCopy_WithoutRunningFfmpeg()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-clone-tail-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var reference = Path.Combine(folder, "voice.wav");
            File.WriteAllBytes(reference, new byte[100]);
            var prepared = CloneReferenceTail.GetPreparedFileName(reference);
            Directory.CreateDirectory(Path.GetDirectoryName(prepared)!);
            File.WriteAllBytes(prepared, new byte[CloneReferenceTail.MinimumPreparedBytes]);
            File.WriteAllText(prepared + ".stamp", CloneReferenceTail.BuildStamp(reference)!);
            var before = File.GetLastWriteTimeUtc(prepared);

            var result = await CloneReferenceTail.PrepareAsync(reference, "Test engine", CancellationToken.None);

            Assert.Equal(prepared, result);
            Assert.Equal(before, File.GetLastWriteTimeUtc(prepared));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void MinimumPreparedBytes_CoversOneSecondPlusThePad()
    {
        // 44-byte header + (1.0 s + 0.4 s) at 24 kHz mono PCM16.
        Assert.Equal(44 + (long)(1.4 * 24000) * 2, CloneReferenceTail.MinimumPreparedBytes);
    }

    [Theory]
    [InlineData("{\"error\":{\"message\":\"Higgs TTS generation reached max_tokens before EOC\",\"type\":\"server_error\"}}", true)]
    [InlineData("Higgs TTS generation reached MAX_TOKENS before EOC", true)]
    [InlineData("unsupported model family hint: higgs_audio_tts", false)]
    [InlineData("max_tokens must be non-negative", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void RunawayGeneration_IsRecognisedFromTheServerError(string? body, bool expected)
    {
        Assert.Equal(expected, HiggsTtsAudioCpp.IsRunawayGenerationError(body));
    }
}
