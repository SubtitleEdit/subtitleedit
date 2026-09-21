using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using UITests.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// Per-line voice cloning stands or falls on which slice of the video each line is cloned from:
/// too short and the clone is mush, too greedy and it clones the neighbouring speaker instead.
/// </summary>
[Collection(TtsSettingsCollection.Name)]
public class PerLineVoiceCloneTests
{
    private static List<Paragraph> Lines(params (double Start, double End)[] times) =>
        times.Select(t => new Paragraph(string.Empty, t.Start * 1000, t.End * 1000)).ToList();

    [Fact]
    public void ALineLongEnoughIsUsedAsItIs()
    {
        var lines = Lines((10, 15));

        var range = PerLineVoiceClone.GetReferenceRange(lines, 0, videoDurationSeconds: 60);

        Assert.Equal(10, range.StartSeconds, 3);
        Assert.Equal(5, range.DurationSeconds, 3);
    }

    [Fact]
    public void AShortLineGrowsIntoTheSilenceAroundIt()
    {
        // One second of speech clones badly; the silence on either side belongs to the same
        // speaker's pause, so it can be borrowed.
        var lines = Lines((10, 11));

        var range = PerLineVoiceClone.GetReferenceRange(lines, 0, videoDurationSeconds: 60);

        Assert.Equal(PerLineVoiceClone.PreferredReferenceSeconds, range.DurationSeconds, 3);
        Assert.Equal(9, range.StartSeconds, 3);
    }

    [Fact]
    public void GrowingStopsAtTheNeighbouringLines()
    {
        // The lines before and after may be other speakers - a reference containing two voices
        // clones the wrong person, which is worse than a short reference.
        var lines = Lines((9.5, 10), (10, 11), (11, 12));

        var range = PerLineVoiceClone.GetReferenceRange(lines, 1, videoDurationSeconds: 60);

        Assert.Equal(10, range.StartSeconds, 3);
        Assert.Equal(1, range.DurationSeconds, 3);
    }

    [Fact]
    public void RoomOnOneSideOnlyIsSpentThere()
    {
        // Previous line ends at 9.8, nothing after: all the growth has to go forwards.
        var lines = Lines((9, 9.8), (10, 11));

        var range = PerLineVoiceClone.GetReferenceRange(lines, 1, videoDurationSeconds: 60);

        Assert.Equal(9.8, range.StartSeconds, 3);
        Assert.Equal(PerLineVoiceClone.PreferredReferenceSeconds, range.DurationSeconds, 3);
    }

    [Fact]
    public void TheClipNeverStartsBeforeTheVideoOrRunsPastItsEnd()
    {
        var atStart = PerLineVoiceClone.GetReferenceRange(Lines((0, 0.5)), 0, videoDurationSeconds: 20);
        Assert.Equal(0, atStart.StartSeconds, 3);

        var atEnd = PerLineVoiceClone.GetReferenceRange(Lines((19.5, 20)), 0, videoDurationSeconds: 20);
        Assert.True(atEnd.StartSeconds + atEnd.DurationSeconds <= 20.001, "the clip runs past the end of the video");
    }

    [Fact]
    public void AnUnknownVideoDurationDoesNotClampTheClip()
    {
        // Duration is 0 when ffprobe told us nothing; falling back to "clamp to 0" would cut every
        // clip to nothing.
        var range = PerLineVoiceClone.GetReferenceRange(Lines((10, 10.5)), 0, videoDurationSeconds: 0);

        Assert.Equal(PerLineVoiceClone.PreferredReferenceSeconds, range.DurationSeconds, 3);
    }

    [Fact]
    public void OnlyEnginesThatTakeAReferencePerCallOfferIt()
    {
        // OmniVoice TTS runs the CLI once per line, so a per-line reference is free. The CrispASR
        // engines read the reference at server start and would reload the model for every line.
        Assert.True(PerLineVoiceClone.CanBeOffered(new OmniVoiceTtsCpp(), "video.mkv"));
        Assert.False(PerLineVoiceClone.CanBeOffered(new OmniVoiceCrispAsr(), "video.mkv"));
        Assert.False(PerLineVoiceClone.CanBeOffered(new EdgeTts(), "video.mkv"));
    }

    [Fact]
    public void WithoutAVideoThereIsNothingToCloneFrom()
    {
        Assert.False(PerLineVoiceClone.CanBeOffered(new OmniVoiceTtsCpp(), string.Empty));
        Assert.False(PerLineVoiceClone.CanBeOffered(new OmniVoiceTtsCpp(), null));
    }

    [Fact]
    public void TheMarkerIsRecognisedAndCountsAsCloning()
    {
        var voice = PerLineVoiceClone.CreateVoice();

        Assert.True(PerLineVoiceClone.IsSelected(voice));
        // It clones every speaker in the video, so the consent gate has to fire for it.
        Assert.True(VoiceCloningConsent.IsCloneVoice(voice));
        // The persisted name is the fixed id, not the translated label shown in the combo.
        Assert.Equal(PerLineCloneVoice.Id, voice.Name);
    }

    [Fact]
    public void AnOrdinaryVoiceIsNotMistakenForTheMarker()
    {
        Assert.False(PerLineVoiceClone.IsSelected(new Voice(new OmniVoice("Ada", "/voices/ada.wav"))));
        Assert.False(PerLineVoiceClone.IsSelected(null));
    }

    [Fact]
    public void AClipBecomesAVoiceOnlyForEnginesThatKnowHowToUseOne()
    {
        using var clips = new ClipFolder();
        var clip = clips.WriteClip("line-0007", "What the video says at that line.");

        var voice = PerLineVoiceClone.MakeVoiceForClip(new OmniVoiceTtsCpp(), clip);

        Assert.NotNull(voice);
        Assert.Equal(clip, Assert.IsType<OmniVoice>(voice!.EngineVoice).FilePath);

        // An engine that does not clone per line (no IPerLineCloneEngine) must come back as null
        // so the caller falls back, rather than getting some other engine's voice type.
        Assert.Null(PerLineVoiceClone.MakeVoiceForClip(new EdgeTts(), "/tmp/refs/line-0007.wav"));
    }

    [Fact]
    public void AQwen3ClipThatCannotBeStagedFallsBackInsteadOfCloning()
    {
        // Qwen3 (CrispASR) speaks only from a copy inside its own voices folder, so a clip it
        // cannot stage - here one that is not there at all - has to come back as null and let the
        // caller fall back, rather than pointing the engine at a file the backend cannot resolve.
        Assert.Null(PerLineVoiceClone.MakeVoiceForClip(new Qwen3TtsCrispAsr(), "/tmp/refs/not-a-clip.wav"));
    }

    [Fact]
    public void AnImportedClipKeepsTheVoiceNameItWasExportedWith()
    {
        // The exported clip may have been renamed to avoid a collision in the export folder; the
        // line should still show the voice it was generated with, not the file it came back as.
        using var clips = new ClipFolder();
        var clip = clips.WriteClip("line-0007_1", "What the video says at that line.");

        var voice = PerLineVoiceClone.MakeVoiceForClip(new OmniVoiceTtsCpp(), clip, "line-0007");

        Assert.Equal("line-0007", voice!.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AnOmniVoiceClipWithoutATranscriptFallsBackInsteadOfFailingTheLine(string? transcript)
    {
        // omnivoice-tts insists on --ref-text, and a blank one makes it drop words from the line.
        // With no original-language subtitle loaded the clips have no transcript, and Speak used
        // to throw "requires a transcript file" for every single line (#15145).
        using var clips = new ClipFolder();
        var clip = clips.WriteClip("line-0001", transcript);

        Assert.Null(PerLineVoiceClone.MakeVoiceForClip(new OmniVoiceTtsCpp(), clip));
    }

    [Fact]
    public void OnlyTheEnginesThatRefuseAClipWithoutATranscriptAskForOne()
    {
        // These three return null from MakePerLineCloneVoice for a clip with no transcript, so a
        // run with no original subtitle loaded is offered speech-to-text up front (#15145).
        Assert.True(PerLineVoiceClone.NeedsTranscript(new OmniVoiceTtsCpp()));
        Assert.True(PerLineVoiceClone.NeedsTranscript(new FireRedTts3AudioCpp()));
        Assert.True(PerLineVoiceClone.NeedsTranscript(new Qwen3TtsCrispAsr()));

        // Cloning from the audio alone, transcribing server-side, or not cloning per line at all.
        Assert.False(PerLineVoiceClone.NeedsTranscript(new HiggsTtsAudioCpp()));
        Assert.False(PerLineVoiceClone.NeedsTranscript(new CosyVoice3CrispAsr()));
        Assert.False(PerLineVoiceClone.NeedsTranscript(new EdgeTts()));
    }

    [Fact]
    public void OnlyClipsWithoutAUsableTranscriptAreSentToSpeechToText()
    {
        using var clips = new ClipFolder();
        var known = clips.WriteClip("line-0001", "What the video says at that line.");
        var missing = clips.WriteClip("line-0002", null);
        var blank = clips.WriteClip("line-0003", "  ");

        Assert.Equal(new[] { missing, blank }, PerLineVoiceClone.GetClipsWithoutTranscript(new[] { known, missing, blank }));
    }

    [Fact]
    public void ATranscribedClipGetsItsSidecarAndBecomesAVoice()
    {
        using var clips = new ClipFolder();
        var heard = clips.WriteClip("line-0001", null);
        var silent = clips.WriteClip("line-0002", null);

        var written = PerLineVoiceClone.WriteTranscripts(
            new[]
            {
                (heard, (IEnumerable<string>)new[] { "Where were you", "yesterday?" }),
                (silent, Array.Empty<string>()),
            },
            TextToSpeechViewModel.BuildRefTextFromTranscription);

        Assert.Equal(1, written);
        Assert.Equal("Where were you yesterday?", File.ReadAllText(Path.ChangeExtension(heard, ".txt")));
        Assert.NotNull(PerLineVoiceClone.MakeVoiceForClip(new OmniVoiceTtsCpp(), heard));

        // Nothing heard, so no sidecar - and that line falls back to an ordinary voice.
        Assert.False(File.Exists(Path.ChangeExtension(silent, ".txt")));
        Assert.Null(PerLineVoiceClone.MakeVoiceForClip(new OmniVoiceTtsCpp(), silent));
    }

    [Fact]
    public void EveryEngineThatOffersPerLineCloningKnowsHowToBuildItsVoice()
    {
        // The capability flag and IPerLineCloneEngine only work as a pair: the flag offers
        // "Clone from video" in the UI, the interface is what builds each line's voice. An engine
        // with the flag but not the interface would offer cloning and then dub every line in the
        // fallback voice. Qwen3 (CrispASR) only raises the flag on its clone model, so that model
        // is pinned while the catalog is swept.
        using var _ = new Qwen3TtsCrispAsrModelScope(Qwen3TtsCrispAsr.ModelKeyClone);

        foreach (var engine in TtsEngineCatalog.CreateVoiceCloningEngines())
        {
            if (engine.SupportsPerLineVoiceCloning)
            {
                Assert.True(engine is IPerLineCloneEngine,
                    $"{engine.Name} sets SupportsPerLineVoiceCloning but does not implement IPerLineCloneEngine");
            }
        }
    }

    [Fact]
    public void OnlyAVoiceThatCanBeRebuiltReportsAReference()
    {
        Assert.Equal("/voices/ada.wav", PerLineVoiceClone.TryGetReferenceClip(new Voice(new OmniVoice("Ada", "/voices/ada.wav"))));

        // Nothing to copy along for a voice that clones from nothing - or for the marker, which
        // has no recording of its own at all.
        Assert.Null(PerLineVoiceClone.TryGetReferenceClip(new Voice(new OmniVoice("Default", string.Empty))));
        Assert.Null(PerLineVoiceClone.TryGetReferenceClip(PerLineVoiceClone.CreateVoice()));
        Assert.Null(PerLineVoiceClone.TryGetReferenceClip(null));
    }

    [Fact]
    public void AFailedLineFallsBackToTheNearestOtherClips()
    {
        // #15020: a line whose own clip makes the engine fail is cloned from a neighbour's.
        var lines = Lines((0, 2), (3, 5), (6, 8), (9, 11), (12, 14));
        var clips = lines.Select((p, i) => (p, i)).ToDictionary(x => x.p, x => $"line-{x.i + 1:0000}.wav");

        var fallbacks = PerLineVoiceClone.GetFallbackReferenceClips(lines, 2, clips, _ => null);

        Assert.Equal(new[] { "line-0002.wav", "line-0004.wav" }, fallbacks);
    }

    [Fact]
    public void FallbackClipsPreferTheSameSpeakerAndSkipLinesWithoutAClip()
    {
        var lines = Lines((0, 2), (3, 5), (6, 8), (9, 11), (12, 14));
        lines[0].Actor = "Anna";
        lines[1].Actor = "Bo";
        lines[2].Actor = "Anna";
        lines[3].Actor = "Bo";
        lines[4].Actor = "Anna";
        var clips = lines.Select((p, i) => (p, i)).Where(x => x.i != 3).ToDictionary(x => x.p, x => $"line-{x.i + 1:0000}.wav");

        var fallbacks = PerLineVoiceClone.GetFallbackReferenceClips(lines, 2, clips, p => p.Actor, maxCount: 3);

        Assert.Equal(new[] { "line-0001.wav", "line-0005.wav", "line-0002.wav" }, fallbacks);
    }

    [Fact]
    public void ALineWithNoOtherClipsHasNoFallback()
    {
        var lines = Lines((0, 2));
        var clips = new Dictionary<Paragraph, string> { [lines[0]] = "line-0001.wav" };

        Assert.Empty(PerLineVoiceClone.GetFallbackReferenceClips(lines, 0, clips, _ => null));
    }

    private sealed class ClipFolder : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), "se-per-line-clone-" + Guid.NewGuid().ToString("N"));

        public ClipFolder()
        {
            Directory.CreateDirectory(_path);
        }

        public string WriteClip(string name, string? transcript)
        {
            var wav = Path.Combine(_path, name + ".wav");
            File.WriteAllText(wav, "not really a wav, and nothing here reads it");
            if (transcript != null)
            {
                File.WriteAllText(Path.ChangeExtension(wav, ".txt"), transcript);
            }

            return wav;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_path, recursive: true);
            }
            catch
            {
                // Best effort.
            }
        }
    }
}
