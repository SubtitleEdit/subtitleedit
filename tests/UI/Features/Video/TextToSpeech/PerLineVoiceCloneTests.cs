using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// Per-line voice cloning stands or falls on which slice of the video each line is cloned from:
/// too short and the clone is mush, too greedy and it clones the neighbouring speaker instead.
/// </summary>
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
        var voice = PerLineVoiceClone.MakeVoiceForClip(new OmniVoiceTtsCpp(), "/tmp/refs/line-0007.wav");

        Assert.NotNull(voice);
        Assert.Equal("/tmp/refs/line-0007.wav", Assert.IsType<OmniVoice>(voice!.EngineVoice).FilePath);

        // An engine that opted in without being taught how to build its voice must return null so
        // the caller falls back, rather than getting some other engine's voice type.
        Assert.Null(PerLineVoiceClone.MakeVoiceForClip(new EdgeTts(), "/tmp/refs/line-0007.wav"));
    }
}
