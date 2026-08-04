using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The rules this engine has to get right are all data rules, and each one has a matching
/// failure mode observed against crispasr 0.8.25:
///  - ship the F16 tokenizer, never the q8_0 one (q8_0 turns voice cloning into noise),
///  - keep the built-in "Default" voice first (this backend, unlike VoxCPM2, has one),
///  - keep the byte sizes exact (a truncated GGUF only fails at server startup).
/// </summary>
public class OmniVoiceCrispAsrTests
{
    [Fact]
    public void RequiredFiles_AlwaysPairTheQuantWithTheF16Tokenizer()
    {
        foreach (var key in new[] { OmniVoiceCrispAsr.ModelKeyQ4K, OmniVoiceCrispAsr.ModelKeyQ8_0, OmniVoiceCrispAsr.ModelKeyF16 })
        {
            var required = OmniVoiceCrispAsr.GetRequiredFileNames(key);

            Assert.Equal(2, required.Length);
            Assert.Equal(OmniVoiceCrispAsr.GetModelFileName(key), required[0]);
            Assert.Equal("omnivoice-tokenizer-f16.gguf", required[1]);
        }
    }

    /// <summary>
    /// omnivoice-tokenizer-q8_0.gguf loads and synthesises, but its encoder tensors cannot be
    /// read back as f32, so an encoded reference voice comes out as noise — and crispasr caches
    /// that bad encoding by audio content. It must never appear anywhere in the engine.
    /// </summary>
    [Fact]
    public void TheQ8TokenizerIsNeverReferenced()
    {
        Assert.Equal("omnivoice-tokenizer-f16.gguf", OmniVoiceCrispAsr.TokenizerFileName);
        Assert.DoesNotContain("tokenizer-q8", OmniVoiceCrispAsr.TokenizerFileName);

        var hash = DownloadHashManager.GetLatestKnownHash(DownloadHashManager.OmniVoiceCrispAsr.TokenizerF16);
        Assert.Equal("710ef610e1f2845c6b7333d5432376b24f2d20d2c54a8cec9bc118d183ecea63", hash);
    }

    [Theory]
    [InlineData(null, "omnivoice-q4_k.gguf")]
    [InlineData("", "omnivoice-q4_k.gguf")]
    [InlineData("nonsense", "omnivoice-q4_k.gguf")]
    [InlineData(OmniVoiceCrispAsr.ModelKeyQ8_0, "omnivoice-q8_0.gguf")]
    [InlineData(OmniVoiceCrispAsr.ModelKeyF16, "omnivoice-f16.gguf")]
    public void GetModelFileName_FallsBackToTheLightestQuant(string? modelKey, string expected)
    {
        // A saved setting from an older/newer build must not resolve to a file that isn't there;
        // anything unrecognised lands on Q4_K rather than throwing mid-synthesis.
        Assert.Equal(expected, OmniVoiceCrispAsr.GetModelFileName(modelKey));
    }

    [Fact]
    public void IsValidLocalModelFile_RejectsATruncatedDownload()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            File.WriteAllBytes(path, new byte[16]);

            // Size is the cheap stand-in for "this GGUF is complete" — the loader would otherwise
            // only fail once the server is already starting.
            Assert.False(OmniVoiceCrispAsr.IsValidLocalModelFile(path, "omnivoice-q4_k.gguf"));

            // A filename with no recorded size is accepted on existence alone.
            Assert.True(OmniVoiceCrispAsr.IsValidLocalModelFile(path, "something-else.gguf"));
        }
        finally
        {
            try { File.Delete(path); } catch { }
        }
    }

    [Fact]
    public async Task Voices_LeadWithTheBuiltInVoice()
    {
        var engine = new OmniVoiceCrispAsr();

        var voices = await engine.GetVoices(string.Empty);

        // Unlike VoxCPM2/MOSS-TTS, this backend synthesises fine with no reference audio, so the
        // combo must offer that as the first entry rather than starting out empty.
        Assert.NotEmpty(voices);
        var first = Assert.IsType<OmniVoiceCrispAsrVoice>(voices[0].EngineVoice);
        Assert.Equal("Default", first.Voice);
        Assert.Equal(string.Empty, first.FilePath);
    }

    [Fact]
    public async Task Engine_UsesTheOmnivoiceBackendAndItsOwnVoiceType()
    {
        var engine = new OmniVoiceCrispAsr();

        Assert.Equal("omnivoice", OmniVoiceCrispAsr.BackendName);
        Assert.Equal("OmniVoice TTS (CrispASR)", engine.Name);
        Assert.True(engine.HasModel);
        Assert.True(engine.HasLanguageParameter);

        // Speaking with the standalone engine's voice type must fail loudly rather than get
        // silently reinterpreted — the two engines share the voice combo.
        var wrongVoice = new Voice(new OmniVoice("Default", string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            engine.Speak("hello", string.Empty, wrongVoice, null, null, null, CancellationToken.None));
    }

    [Fact]
    public void ModelHashes_AreRegisteredForEveryQuant()
    {
        foreach (var key in new[]
                 {
                     DownloadHashManager.OmniVoiceCrispAsr.ModelQ4K,
                     DownloadHashManager.OmniVoiceCrispAsr.ModelQ8_0,
                     DownloadHashManager.OmniVoiceCrispAsr.ModelF16,
                     DownloadHashManager.OmniVoiceCrispAsr.TokenizerF16,
                 })
        {
            var hash = DownloadHashManager.GetLatestKnownHash(key);
            Assert.False(string.IsNullOrEmpty(hash), $"no hash registered for {key}");
            Assert.Equal(64, hash!.Length);
        }
    }
}
