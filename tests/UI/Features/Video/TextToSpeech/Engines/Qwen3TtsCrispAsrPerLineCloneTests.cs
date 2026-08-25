using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Per-line voice cloning on Qwen3 (CrispASR): which model it is offered for, and what "staging"
/// a line's reference has to leave in the voices folder for the backend to find it.
/// </summary>
/// <remarks>
/// The backend resolves a request's <c>voice</c> as a bare name inside --voice-dir and auto-loads
/// the matching <c>.txt</c> as ref-text, so a reference that is not in that folder - or has no
/// sidecar - is not a reference at all. That is the whole reason staging exists.
/// </remarks>
[Collection(TtsSettingsCollection.Name)]
public class Qwen3TtsCrispAsrPerLineCloneTests
{
    [Theory]
    [InlineData(Qwen3TtsCrispAsr.ModelKeyClone, true)]
    [InlineData(Qwen3TtsCrispAsr.ModelKeyVoiceDesign, false)]
    [InlineData(Qwen3TtsCrispAsr.ModelKeyCustomVoice, false)]
    public void OnlyTheCloneModelOffersIt(string modelKey, bool expected)
    {
        // VoiceDesign speaks from the instruction and CustomVoice from a fixed speaker list -
        // both ignore a reference WAV, so offering "Clone from video" there would do nothing.
        using var _ = new Qwen3TtsCrispAsrModelScope(modelKey);

        Assert.Equal(expected, new Qwen3TtsCrispAsr().SupportsPerLineVoiceCloning);
        Assert.Equal(expected, PerLineVoiceClone.CanBeOffered(new Qwen3TtsCrispAsr(), "/videos/movie.mkv"));
    }

    [Fact]
    public void StagingCopiesTheClipAndItsTranscriptIntoTheVoicesFolder()
    {
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        var clip = clips.WriteClip("line-0007", "Nothing travels faster than light.");

        var staged = Qwen3TtsCrispAsr.StagePerLineReferenceIn(clip, voices.Path);

        Assert.NotNull(staged);
        Assert.Equal(voices.Path, Path.GetDirectoryName(staged));
        // The name is what goes into the request, so it has to survive as a file name here.
        Assert.True(File.Exists(staged));
        Assert.Equal("Nothing travels faster than light.", File.ReadAllText(Path.ChangeExtension(staged!, ".txt")));
        Assert.True(Qwen3TtsCrispAsr.IsStagedPerLineReference(staged!));
    }

    [Fact]
    public void AClipWithNoTranscriptIsNotStagedAtAll()
    {
        // The backend answers a reference without ref-text with an HTTP 500, so the line is
        // better off falling back to an ordinary voice - which is what null means to the caller.
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        var clip = clips.WriteClip("line-0008", transcript: null);

        Assert.Null(Qwen3TtsCrispAsr.StagePerLineReferenceIn(clip, voices.Path));
        Assert.Empty(Directory.GetFiles(voices.Path));
    }

    [Fact]
    public void ClearingRemovesTheStagedReferencesAndLeavesImportedVoicesAlone()
    {
        // A run over a shorter subtitle than the last must not inherit the extra lines, but the
        // user's own imports live in the same folder and are not ours to delete.
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        File.WriteAllText(Path.Combine(voices.Path, "ada.wav"), "imported");
        File.WriteAllText(Path.Combine(voices.Path, "ada.txt"), "Imported voice.");
        Qwen3TtsCrispAsr.StagePerLineReferenceIn(clips.WriteClip("line-0001", "One."), voices.Path);
        Qwen3TtsCrispAsr.StagePerLineReferenceIn(clips.WriteClip("line-0002", "Two."), voices.Path);

        Qwen3TtsCrispAsr.ClearStagedPerLineReferencesIn(voices.Path);

        Assert.Equal(
            new[] { "ada.txt", "ada.wav" },
            Directory.GetFiles(voices.Path).Select(Path.GetFileName).Order().ToArray());
    }

    [Fact]
    public void AStagedReferenceIsNotMistakenForAnImportedVoice()
    {
        Assert.True(Qwen3TtsCrispAsr.IsStagedPerLineReference("/voices/" + Qwen3TtsCrispAsr.PerLineReferencePrefix + "line-0003.wav"));
        Assert.False(Qwen3TtsCrispAsr.IsStagedPerLineReference("/voices/ada.wav"));
    }

    [Fact]
    public void ReStagingAnExportedReferenceDoesNotPileUpPrefixes()
    {
        // An exported session carries the staged name, and importing it stages it again. Without
        // stripping, every round trip would add another prefix to the file name.
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        var exported = clips.WriteClip(Qwen3TtsCrispAsr.PerLineReferencePrefix + "line-0004", "Once more.");

        var staged = Qwen3TtsCrispAsr.StagePerLineReferenceIn(exported, voices.Path);

        Assert.Equal(
            Qwen3TtsCrispAsr.PerLineReferencePrefix + "line-0004.wav",
            Path.GetFileName(staged));
    }

    [Fact]
    public void TheVoiceIsNamedForTheLineButSpeaksFromTheStagedCopy()
    {
        // The two are separate on purpose: Speak sends the FilePath's bare name as the request's
        // `voice` (it is the --voice-dir lookup key), which leaves the name free to be the one
        // the row shows - so an imported session keeps the name a line was generated with.
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        var clip = clips.WriteClip("line-0009", "Two roads diverged in a wood.");

        var staged = Qwen3TtsCrispAsr.StagePerLineReferenceIn(clip, voices.Path);
        var voice = new Voice(new Qwen3TtsVoice("Morgan", staged!));

        var engineVoice = Assert.IsType<Qwen3TtsVoice>(voice.EngineVoice);
        Assert.Equal(staged, engineVoice.FilePath);
        Assert.Equal("Morgan", engineVoice.Voice);

        // Export and regenerate both find the recording through this, or an imported session
        // could never be re-dubbed.
        Assert.Equal(staged, PerLineVoiceClone.TryGetReferenceClip(voice));
    }

    [Fact]
    public void AVoiceWithNoRecordingHasNoReferenceToExport()
    {
        // VoiceDesign and CustomVoice voices carry no file - there is nothing to copy for them.
        Assert.Null(PerLineVoiceClone.TryGetReferenceClip(new Voice(new Qwen3TtsVoice("vivian", string.Empty))));
    }

    private sealed class TempFolder : IDisposable
    {
        public string Path { get; } =
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SeQwen3PerLine_" + Guid.NewGuid().ToString("N"));

        public TempFolder() => Directory.CreateDirectory(Path);

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
                Directory.Delete(Path, true);
            }
            catch
            {
                // temp folder - nothing depends on it being gone
            }
        }
    }
}

/// <summary>
/// Restores the saved Qwen3 (CrispASR) model after a test picks one, since the engine reads it
/// from the shared settings rather than taking it as an argument.
/// </summary>
internal sealed class Qwen3TtsCrispAsrModelScope : IDisposable
{
    private readonly string _original;

    public Qwen3TtsCrispAsrModelScope(string modelKey)
    {
        _original = Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel;
        Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel = modelKey;
    }

    public void Dispose()
    {
        Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel = _original;
    }
}
