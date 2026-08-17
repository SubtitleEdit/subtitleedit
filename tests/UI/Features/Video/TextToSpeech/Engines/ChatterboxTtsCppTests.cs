using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Chatterbox is the one SE engine the crispasr server classifies as voice cloning, because its
/// <c>voice</c> field keeps the <c>.wav</c> extension — the server's literal clone test. That makes
/// it the engine that hard-requires both attestations, so its payload is worth pinning down.
/// </summary>
[Collection(TtsSettingsCollection.Name)]
public class ChatterboxTtsCppTests
{
    [Fact]
    public void BuildSpeakPayload_CloningWithTargetLanguage_SendsSourceLang()
    {
        // Both sides present is what makes the backend go cross-lingual (CrispASR v0.8.29 #329).
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hallo", "/voices/Arnold.wav", "de", "en");

        Assert.Equal("de", payload["language"]);
        Assert.Equal("en", payload["source_lang"]);
    }

    [Fact]
    public void BuildSpeakPayload_WithoutTargetLanguage_SendsNoSourceLang()
    {
        // The reference language on its own tells the backend nothing to act on.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", "/voices/Arnold.wav", string.Empty, "en");

        Assert.False(payload.ContainsKey("source_lang"));
    }

    [Fact]
    public void BuildSpeakPayload_WithBakedDefaultVoice_SendsNoSourceLang()
    {
        // Nothing is being cloned from, so there is no reference language to declare.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hallo", string.Empty, "de", "en");

        Assert.False(payload.ContainsKey("source_lang"));
    }

    [Fact]
    public void TryReadReferenceTranscript_ReadsTheSidecarBesideTheWav()
    {
        var wav = Path.Combine(Path.GetTempPath(), $"chatterbox-ref-{Guid.NewGuid():N}.wav");
        var sidecar = Path.ChangeExtension(wav, ".txt");
        try
        {
            File.WriteAllText(wav, string.Empty);
            Assert.Null(ChatterboxTtsCpp.TryReadReferenceTranscript(wav));

            File.WriteAllText(sidecar, "  This is what the reference says.  ");
            Assert.Equal("This is what the reference says.", ChatterboxTtsCpp.TryReadReferenceTranscript(wav));

            File.WriteAllText(sidecar, "   ");
            Assert.Null(ChatterboxTtsCpp.TryReadReferenceTranscript(wav));
        }
        finally
        {
            File.Delete(wav);
            File.Delete(sidecar);
        }
    }

    [Fact]
    public void BuildSpeakPayload_KeepsWavExtensionOnVoiceName()
    {
        // The chatterbox backend does not append an extension, and the server needs the .wav to
        // recognise the request as cloning at all.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", "/voices/Arnold.wav");

        Assert.Equal("Arnold.wav", payload["voice"]);
    }

    [Fact]
    public void BuildSpeakPayload_SendsBareFileNameNotPath()
    {
        // A path separator is rejected outright with HTTP 400 invalid_voice.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", Path.Combine("some", "dir", "Arnold.wav"));

        Assert.Equal("Arnold.wav", payload["voice"]);
    }

    [Fact]
    public void BuildSpeakPayload_WhenCloningAccepted_SendsBothAttestations()
    {
        using var _ = new AcceptVoiceCloningScope(true);

        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", "/voices/Arnold.wav");

        Assert.True(payload.ContainsKey("consent_attestation"));
        Assert.True(payload.ContainsKey("marking_attestation"));
        Assert.Equal(false, payload["spoken_disclaimer"]);
    }

    [Fact]
    public void BuildSpeakPayload_WhenCloningNotAccepted_SendsNoAttestations()
    {
        using var _ = new AcceptVoiceCloningScope(false);

        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", "/voices/Arnold.wav");

        Assert.Equal("Arnold.wav", payload["voice"]);
        Assert.False(payload.ContainsKey("consent_attestation"));
        Assert.False(payload.ContainsKey("marking_attestation"));
        Assert.False(payload.ContainsKey("spoken_disclaimer"));
    }

    [Fact]
    public void BuildSpeakPayload_WithBakedDefaultVoice_IsNotCloning()
    {
        // No reference WAV means no cloning, so neither a `voice` field nor an attestation.
        using var _ = new AcceptVoiceCloningScope(true);

        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", string.Empty);

        Assert.False(payload.ContainsKey("voice"));
        Assert.False(payload.ContainsKey("consent_attestation"));
        Assert.False(payload.ContainsKey("marking_attestation"));
        Assert.Equal("hello", payload["input"]);
    }

    [Fact]
    public void BuildSpeakPayload_WithLanguage_SendsLanguageField()
    {
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("bonjour", string.Empty, "fr");

        Assert.Equal("fr", payload["language"]);
    }

    [Fact]
    public void BuildSpeakPayload_WithoutLanguage_SendsNoLanguageField()
    {
        // No language (Auto, or the Turbo model) must keep the payload identical to the
        // pre-multilingual behaviour — the server treats a missing field as language-agnostic.
        var payload = ChatterboxTtsCpp.BuildSpeakPayload("hello", string.Empty);

        Assert.False(payload.ContainsKey("language"));
    }

    [Fact]
    public void Languages_LeadWithAutoThenTheTwentyThreeSupportedLanguages()
    {
        // "Auto" first so a combo falling back to its first entry reproduces the
        // pre-language-selection behaviour (no field sent).
        var all = ChatterboxLanguages.All;

        Assert.Equal(24, all.Length);
        Assert.Equal("Auto", all[0].Name);
        Assert.Equal(string.Empty, all[0].Code);
    }

    [Theory]
    [InlineData("French", "fr")]
    [InlineData("German", "de")]
    [InlineData("Chinese", "zh")]
    public void Languages_ResolveToIsoCode(string displayName, string expectedArg)
    {
        var language = ChatterboxLanguages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedArg, ChatterboxLanguages.ResolveLanguageArg(language));
    }

    [Fact]
    public void Languages_AutoResolvesToEmpty()
    {
        Assert.Equal(string.Empty, ChatterboxLanguages.ResolveLanguageArg(ChatterboxLanguages.Auto));
    }

    [Fact]
    public void Languages_ForeignEngineCodeIsDropped()
    {
        // A language object left over from another engine (e.g. OmniVoice's ISO 639-3 ids)
        // must not leak onto the wire.
        var foreign = new TtsLanguage("Standard Arabic", "arb");

        Assert.Equal(string.Empty, ChatterboxLanguages.ResolveLanguageArg(foreign));
    }

    [Theory]
    [InlineData(24000, 1, 16, true)]   // exactly what the backend clones from
    [InlineData(48000, 1, 16, false)]  // #13508: the reported reference - right shape, wrong rate
    [InlineData(16000, 1, 16, false)]  // only the partial M2+M3 path upstream, so still converted
    [InlineData(24000, 2, 16, false)]  // stereo
    [InlineData(24000, 1, 8, false)]   // PCM8
    [InlineData(24000, 1, 24, false)]  // PCM24
    public void IsCloneReadyReferenceWav_AcceptsOnly24kHzMono(int sampleRate, int channels, int bitsPerSample, bool expected)
    {
        var path = WriteTempWav(MakeWav(sampleRate, channels, bitsPerSample));
        try
        {
            Assert.Equal(expected, ChatterboxTtsCpp.IsCloneReadyReferenceWav(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void IsCloneReadyReferenceWav_Accepts24kHzMonoFloat32()
    {
        // WAVE_FORMAT_IEEE_FLOAT is the backend's other accepted reference format, so a file
        // already in it must not be re-encoded on every synthesis.
        var path = WriteTempWav(MakeWav(24000, 1, 32, audioFormat: 3));
        try
        {
            Assert.True(ChatterboxTtsCpp.IsCloneReadyReferenceWav(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void IsCloneReadyReferenceWav_TreatsUnreadableFileAsNeedingConversion()
    {
        // A non-RIFF file (an MP3 renamed .wav, a truncated download) must fall to the ffmpeg
        // path rather than be sent to a backend that cannot open it.
        var path = WriteTempWav("this is not a wav file"u8.ToArray());
        try
        {
            Assert.False(ChatterboxTtsCpp.IsCloneReadyReferenceWav(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void EnsureCloneReferenceIsUsable_LeavesAGoodReferenceByteIdentical()
    {
        // The repair rewrites the file in place, so a reference that is already right must not
        // be touched at all - re-encoding on every synthesis would degrade it a generation at a time.
        var bytes = MakeWav(24000, 1, 16);
        var path = WriteTempWav(bytes);
        try
        {
            Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(path));
            Assert.Equal(bytes, File.ReadAllBytes(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void EnsureCloneReferenceIsUsable_RepairsAWrongRateReferenceInPlace()
    {
        // The #13508 case: a 48 kHz WAV that reached the voices folder without passing through
        // ImportVoice. It has to come back out as something the backend can clone from, under
        // the same file name - the `voice` field is that name.
        var path = WriteTempWav(MakeWav(48000, 2, 16));
        try
        {
            var repaired = ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(path);

            // ffmpeg is not on every box (CI images, a fresh dev machine). The repair then fails,
            // and the one thing that must still hold is that the original was left alone rather
            // than replaced by a half-written file.
            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 44);
            if (repaired)
            {
                Assert.True(ChatterboxTtsCpp.IsCloneReadyReferenceWav(path));
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void EnsureCloneReferenceIsUsable_WithNoReference_IsNotAFailure()
    {
        // The baked default voice sends no `voice` field, so there is nothing to check.
        Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(string.Empty));
        Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(null));
    }

    [Fact]
    public void EnsureCloneReferenceIsUsable_WithMissingFile_IsNotAFailure()
    {
        // A voice deleted behind SE's back is the server's error to report, not a conversion failure.
        Assert.True(ChatterboxTtsCpp.EnsureCloneReferenceIsUsable(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.wav")));
    }

    [Theory]
    [InlineData("chatterbox: native WAV cloning failed.\n  Tried 24 kHz: sample rate 48000 not supported (need 24000); pre-convert or use the python baker", true)]
    [InlineData("crispasr-server: listening on 127.0.0.1:8836", false)]
    public void LooksLikeCloneReferenceRejected_MatchesTheBackendsRefusal(string serverLog, bool expected)
    {
        // The HTTP body only says "backend returned empty audio" - the reason is log-only, and
        // this is what turns it into something the user can act on.
        Assert.Equal(expected, ChatterboxTtsCpp.LooksLikeCloneReferenceRejected(serverLog));
    }

    [Theory]
    // Verbatim from the crash in #13572 - the CUDA build dying at the first AR step of the
    // request that switches cloned voice.
    [InlineData("chatterbox[ar]: step=0 tok=3704\nCUDA error: invalid argument\n  current device: 0, in function ggml_cuda_cpy at D:\\a\\CrispASR\\CrispASR\\ggml\\src\\ggml-cuda\\cpy.cu:474", true)]
    // A CUDA fault in another op still counts: the advice (use the Vulkan build) is the same.
    [InlineData("CUDA error: out of memory", true)]
    // The CPU/Vulkan assert is a different bug with different advice - it must not match here.
    [InlineData("ggml-backend.cpp:349: GGML_ASSERT(offset + size <= ggml_nbytes(tensor) && \"tensor read out of bounds\") failed", false)]
    [InlineData("crispasr-server: synthesized 13.4s audio in 6.87s (RTF=0.51)", false)]
    public void LooksLikeCudaBackendCrash_MatchesOnlyTheCudaFault(string serverLog, bool expected)
    {
        Assert.Equal(expected, ChatterboxTtsCpp.LooksLikeCudaBackendCrash(serverLog));
    }

    [Fact]
    public void RemoveSupersededBaseModels_DeletesTheUnversionedBasePairOnly()
    {
        // The chatterbox-v3-* pair replaced the unversioned Base GGUFs, so those are dead weight
        // once it is downloaded - up to ~3.4 GB for a user who had all three quantizations.
        // Turbo keeps its own unversioned names and must survive.
        var folder = Path.Combine(Path.GetTempPath(), $"chatterbox-cleanup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        try
        {
            string[] superseded =
            {
                "chatterbox-t3-q8_0.gguf", "chatterbox-s3gen-q8_0.gguf",
                "chatterbox-t3-f16.gguf", "chatterbox-s3gen-f16.gguf",
                "chatterbox-t3-q4_k.gguf", "chatterbox-s3gen-q4_k.gguf",
            };
            string[] kept =
            {
                "chatterbox-turbo-t3-q8_0.gguf", "chatterbox-turbo-s3gen-q8_0.gguf",
                ChatterboxTtsCppDownloadService.BaseT3FileName,
                ChatterboxTtsCppDownloadService.BaseS3GenFileName,
            };

            foreach (var name in superseded.Concat(kept))
            {
                File.WriteAllText(Path.Combine(folder, name), "x");
            }

            ChatterboxTtsCppDownloadService.RemoveSupersededBaseModels(folder);

            foreach (var name in superseded)
            {
                Assert.False(File.Exists(Path.Combine(folder, name)), name);
            }

            foreach (var name in kept)
            {
                Assert.True(File.Exists(Path.Combine(folder, name)), name);
            }
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Theory]
    [InlineData("Base", "chatterbox-v3-t3-q8_0.gguf", "chatterbox-v3-s3gen-q8_0.gguf", "chatterbox")]
    [InlineData("Base F16", "chatterbox-v3-t3-f16.gguf", "chatterbox-v3-s3gen-f16.gguf", "chatterbox")]
    [InlineData("Base Q4_K", "chatterbox-v3-t3-q4_k.gguf", "chatterbox-v3-s3gen-q4_k.gguf", "chatterbox")]
    [InlineData("Turbo", "chatterbox-turbo-t3-q8_0.gguf", "chatterbox-turbo-s3gen-q8_0.gguf", "chatterbox-turbo")]
    public void EachModelKey_MapsToItsOwnGgufPairAndBackend(string modelKey, string t3, string s3gen, string backend)
    {
        // The Base quantizations are the same weights at different precision, so they all run
        // on the plain chatterbox backend - only Turbo is a separate backend.
        Assert.Equal(t3, ChatterboxTtsCppDownloadService.GetT3FileName(modelKey));
        Assert.Equal(s3gen, ChatterboxTtsCppDownloadService.GetS3GenFileName(modelKey));
        Assert.Equal(backend, ChatterboxTtsCppDownloadService.GetBackendName(modelKey));
    }

    [Theory]
    [InlineData("base f16", "Base F16")]
    [InlineData("BASE Q4_K", "Base Q4_K")]
    [InlineData("turbo", "Turbo")]
    [InlineData("Base", "Base")]
    [InlineData("", "Base")]
    [InlineData(null, "Base")]
    [InlineData("something removed in a later release", "Base")]
    public void ResolveModelKey_IsCaseInsensitiveAndFallsBackToBase(string? saved, string expected)
    {
        // A settings file written by a newer/older SE must not leave the engine with a model
        // key it cannot map to files - unknown keys degrade to the default Base pair.
        Assert.Equal(expected, ChatterboxTtsCppDownloadService.ResolveModelKey(saved));
    }

    [Fact]
    public void AllModelKeys_AreDistinctAndResolveToThemselves()
    {
        var keys = ChatterboxTtsCppDownloadService.GetAllModelKeys();

        Assert.Equal(keys.Length, keys.Distinct().Count());
        Assert.All(keys, k => Assert.Equal(k, ChatterboxTtsCppDownloadService.ResolveModelKey(k)));

        // Every key needs its own file pair, or one model would silently overwrite another's
        // download in the shared models folder.
        var files = keys.SelectMany(k => new[]
        {
            ChatterboxTtsCppDownloadService.GetT3FileName(k),
            ChatterboxTtsCppDownloadService.GetS3GenFileName(k),
        }).ToList();
        Assert.Equal(files.Count, files.Distinct().Count());
    }
    private static string WriteTempWav(byte[] bytes)
    {
        var path = Path.Combine(Path.GetTempPath(), $"chatterbox-ref-test-{Guid.NewGuid():N}.wav");
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static byte[] MakeWav(int sampleRate, int channels, int bitsPerSample, int audioFormat = 1)
    {
        const int samples = 100;
        var blockAlign = channels * ((bitsPerSample + 7) / 8);
        var dataBytes = samples * blockAlign;

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataBytes);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)audioFormat);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * blockAlign);
        writer.Write((short)blockAlign);
        writer.Write((short)bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(dataBytes);
        writer.Write(new byte[dataBytes]);
        writer.Flush();

        return stream.ToArray();
    }
}
