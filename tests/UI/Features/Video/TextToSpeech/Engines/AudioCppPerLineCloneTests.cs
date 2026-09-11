using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Per-line voice cloning on the audio.cpp engines (IndexTTS 2.5, Higgs Audio v3, Fish Audio
/// S2 Pro, FireRedTTS3). Their server takes the reference as a per-request path, so nothing is staged: the
/// voice for a line is the cut clip itself.
/// </summary>
/// <remarks>
/// The engine-specific rules are about the transcript. Fish S2 Pro takes a blank placeholder
/// when there is none; Higgs and IndexTTS clone from the audio alone; FireRedTTS3 cannot - its
/// prompt pairs the reference audio with its transcript and without one the model returns
/// noise (#14480) - so a clip with no usable .txt sidecar must not become a FireRed voice at
/// all, and the line falls back to an ordinary voice instead of the run producing garbage.
/// </remarks>
public class AudioCppPerLineCloneTests
{
    public static IEnumerable<object[]> Engines()
    {
        yield return new object[] { new IndexTts25AudioCpp() };
        yield return new object[] { new HiggsTtsAudioCpp() };
        yield return new object[] { new FishTtsAudioCpp() };
        yield return new object[] { new FireRedTts3AudioCpp() };
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
    // The shared voice pack's Wikimedia attribution blurb is a sidecar, not a transcript.
    [InlineData("Wikimedia Commons, CC BY-SA 4.0, https://commons.wikimedia.org/wiki/File:Speech.ogg")]
    public void FireRedWithoutAUsableTranscriptDoesNotCloneTheLine(string? transcript)
    {
        // With no text to pair the reference audio with, FireRedTTS3 produced a second or two
        // of noise on every seed (#14480) - so the clip is refused and the line falls back to
        // an ordinary voice.
        using var clips = new TempFolder();
        var clip = clips.WriteClip("line-0010", transcript);

        Assert.Null(PerLineVoiceClone.MakeVoiceForClip(new FireRedTts3AudioCpp(), clip));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FishWithoutATranscriptStillClones(string? transcript)
    {
        // A per-line clip has no transcript when no original-language subtitle is loaded. That
        // used to drop the clip (Speak threw on an empty sidecar); now Speak sends a blank
        // placeholder reference_text instead, which the server accepts and clones from fine -
        // whereas writing the (translated) line as the transcript made the model replay the
        // clip instead of speaking the line (#14480). So the clip is a voice.
        using var clips = new TempFolder();
        var clip = clips.WriteClip("line-0009", transcript);

        Assert.NotNull(PerLineVoiceClone.MakeVoiceForClip(new FishTtsAudioCpp(), clip));
        Assert.Equal(" ", FishTtsAudioCpp.UnknownReferenceTextPlaceholder);
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
