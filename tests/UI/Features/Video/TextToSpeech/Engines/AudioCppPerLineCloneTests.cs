using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Per-line voice cloning on the audio.cpp engines (IndexTTS 2.5, Higgs Audio v3, Fish Audio
/// S2 Pro). Their server takes the reference as a per-request path, so nothing is staged: the
/// voice for a line is the cut clip itself.
/// </summary>
/// <remarks>
/// The one engine-specific rule is Fish's: S2 Pro refuses a reference without a transcript, so
/// a clip with no .txt sidecar must not become a voice at all - the line falls back to an
/// ordinary voice instead of the whole run failing on it.
/// </remarks>
public class AudioCppPerLineCloneTests
{
    public static IEnumerable<object[]> Engines()
    {
        yield return new object[] { new IndexTts25AudioCpp() };
        yield return new object[] { new HiggsTtsAudioCpp() };
        yield return new object[] { new FishTtsAudioCpp() };
    }

    [Theory]
    [MemberData(nameof(Engines))]
    public void ItIsOfferedWithAVideo(ITtsEngine engine)
    {
        Assert.True(engine.SupportsPerLineVoiceCloning);
        Assert.True(PerLineVoiceClone.CanBeOffered(engine, "/videos/movie.mkv"));
        Assert.False(PerLineVoiceClone.CanBeOffered(engine, string.Empty));
    }

    [Theory]
    [MemberData(nameof(Engines))]
    public void AClipWithATranscriptBecomesAVoicePointingAtTheClip(ITtsEngine engine)
    {
        using var clips = new TempFolder();
        var clip = clips.WriteClip("line-0007", "Nothing travels faster than light.");

        var voice = PerLineVoiceClone.MakeVoiceForClip(engine, clip, "line-0007");

        Assert.NotNull(voice);
        var indexVoice = Assert.IsType<IndexTtsVoice>(voice!.EngineVoice);
        Assert.Equal(clip, indexVoice.FilePath);
        Assert.Equal("line-0007", indexVoice.Voice);
        // Export and regenerate have to find the recording again through the voice.
        Assert.Equal(clip, PerLineVoiceClone.TryGetReferenceClip(voice));
    }

    [Fact]
    public void HiggsAndIndexTtsCloneFromTheAudioAloneSoAMissingTranscriptIsFine()
    {
        using var clips = new TempFolder();
        var clip = clips.WriteClip("line-0008", transcript: null);

        Assert.NotNull(PerLineVoiceClone.MakeVoiceForClip(new HiggsTtsAudioCpp(), clip));
        Assert.NotNull(PerLineVoiceClone.MakeVoiceForClip(new IndexTts25AudioCpp(), clip));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FishWithoutATranscriptFallsBackInsteadOfCloning(string? transcript)
    {
        // audio.cpp answers a Fish voice_ref without reference_text with an HTTP 500, and Speak
        // throws on an empty sidecar - so the clip must not be handed out as a voice.
        using var clips = new TempFolder();
        var clip = clips.WriteClip("line-0009", transcript);

        Assert.Null(PerLineVoiceClone.MakeVoiceForClip(new FishTtsAudioCpp(), clip));
    }

    [Fact]
    public void AVoiceThatClonesFromNothingReportsNoReference()
    {
        Assert.Null(PerLineVoiceClone.TryGetReferenceClip(new Voice(new IndexTtsVoice("Default", string.Empty))));
    }

    private sealed class TempFolder : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se-audiocpp-per-line-" + Guid.NewGuid().ToString("N"));

        public TempFolder()
        {
            Directory.CreateDirectory(Path);
        }

        public string WriteClip(string name, string? transcript)
        {
            var wav = System.IO.Path.Combine(Path, name + ".wav");
            File.WriteAllText(wav, "not really a wav, and nothing here reads it");
            if (transcript != null)
            {
                File.WriteAllText(System.IO.Path.ChangeExtension(wav, ".txt"), transcript);
            }

            return wav;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best effort.
            }
        }
    }
}
