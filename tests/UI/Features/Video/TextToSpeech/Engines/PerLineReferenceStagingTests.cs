using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Per-line voice cloning on VibeVoice and MOSS-TTS (CrispASR): both backends resolve a
/// request's <c>voice</c> as a bare stem inside --voice-dir, so a line's reference clip has to
/// be staged into the voices folder first - the shared <see cref="PerLineReferenceStaging"/>.
/// </summary>
public class PerLineReferenceStagingTests
{
    [Fact]
    public void BothEnginesOfferPerLineCloning()
    {
        Assert.True(PerLineVoiceClone.CanBeOffered(new VibeVoiceCrispAsr(), "/videos/movie.mkv"));
        Assert.True(PerLineVoiceClone.CanBeOffered(new MossTtsCrispAsr(), "/videos/movie.mkv"));
        Assert.IsAssignableFrom<IPerLineCloneEngine>(new VibeVoiceCrispAsr());
        Assert.IsAssignableFrom<IPerLineCloneEngine>(new MossTtsCrispAsr());
    }

    [Fact]
    public void StagingCopiesTheClipAndItsTranscriptIntoTheVoicesFolder()
    {
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        var clip = clips.WriteClip("line-0007", "Nothing travels faster than light.");

        var staged = PerLineReferenceStaging.Stage(clip, voices.Path, "test");

        Assert.NotNull(staged);
        Assert.Equal(voices.Path, Path.GetDirectoryName(staged));
        Assert.True(File.Exists(staged));
        Assert.Equal("Nothing travels faster than light.", File.ReadAllText(Path.ChangeExtension(staged!, ".txt")));
        Assert.True(PerLineReferenceStaging.IsStaged(staged!));
        Assert.StartsWith(PerLineReferenceStaging.Prefix, Path.GetFileName(staged));
    }

    [Fact]
    public void AClipWithoutTranscriptIsStillStaged()
    {
        // Both engines clone from audio alone, so the sidecar is a nicety, not a requirement -
        // and a stale one from an earlier run must not be left describing a different clip.
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        File.WriteAllText(Path.Combine(voices.Path, PerLineReferenceStaging.Prefix + "line-0008.txt"), "stale");

        var staged = PerLineReferenceStaging.Stage(clips.WriteClip("line-0008", transcript: null), voices.Path, "test");

        Assert.NotNull(staged);
        Assert.True(File.Exists(staged));
        Assert.False(File.Exists(Path.ChangeExtension(staged!, ".txt")));
    }

    [Fact]
    public void AMissingClipIsNotStaged()
    {
        using var voices = new TempFolder();

        Assert.Null(PerLineReferenceStaging.Stage(Path.Combine(voices.Path, "nope.wav"), voices.Path, "test"));
        Assert.Empty(Directory.GetFiles(voices.Path));
    }

    [Fact]
    public void ReStagingAnAlreadyStagedClipDoesNotStackThePrefix()
    {
        // Regenerate hands back the staged copy itself; an exported session carries the staged
        // name - neither may end up as "se-per-line-se-per-line-…".
        using var voices = new TempFolder();
        var clip = voices.WriteClip(PerLineReferenceStaging.Prefix + "line-0001", "One.");

        var staged = PerLineReferenceStaging.Stage(clip, voices.Path, "test");

        Assert.Equal(clip, staged);
        Assert.Single(Directory.GetFiles(voices.Path, "*.wav"));
    }

    [Fact]
    public void ClearingRemovesTheStagedReferencesAndLeavesImportedVoicesAlone()
    {
        using var clips = new TempFolder();
        using var voices = new TempFolder();
        File.WriteAllText(Path.Combine(voices.Path, "ada.wav"), "imported");
        File.WriteAllText(Path.Combine(voices.Path, "ada.txt"), "Imported voice.");
        PerLineReferenceStaging.Stage(clips.WriteClip("line-0001", "One."), voices.Path, "test");
        PerLineReferenceStaging.Stage(clips.WriteClip("line-0002", null), voices.Path, "test");

        PerLineReferenceStaging.Clear(voices.Path, "test");

        Assert.Equal(
            new[] { "ada.txt", "ada.wav" },
            Directory.GetFiles(voices.Path).Select(Path.GetFileName).Order().ToArray());
    }

    [Fact]
    public void TheVoicePointsAtTheStagedCopy()
    {
        var vibe = new VibeVoiceCrispAsr();
        var moss = new MossTtsCrispAsr();

        Assert.Equal("/voices/se-per-line-line-0003.wav",
            vibe.GetPerLineReferenceClip(new Voice(new VibeVoice("Ada", "/voices/se-per-line-line-0003.wav"))));
        Assert.Equal("/voices/se-per-line-line-0003.wav",
            moss.GetPerLineReferenceClip(new Voice(new MossTtsVoice("Ada", "/voices/se-per-line-line-0003.wav"))));
        Assert.Null(vibe.GetPerLineReferenceClip(new Voice(new MossTtsVoice("Ada", "/voices/x.wav"))));
        Assert.Null(moss.GetPerLineReferenceClip(new Voice(new VibeVoice("Ada", "/voices/x.wav"))));
    }

    private sealed class TempFolder : IDisposable
    {
        public string Path { get; } =
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SePerLineStaging_" + Guid.NewGuid().ToString("N"));

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
