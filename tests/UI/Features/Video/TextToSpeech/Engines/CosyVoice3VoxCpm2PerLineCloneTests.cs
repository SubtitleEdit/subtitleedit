using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Per-line voice cloning on CosyVoice3 and VoxCPM2 (CrispASR): both backends open a
/// per-request <c>voice</c> as a literal path relative to the server's working directory, which
/// SE points at the voices folder, so the request carries the staged clip's bare file name.
/// </summary>
public class CosyVoice3VoxCpm2PerLineCloneTests
{
    [Fact]
    public void BothEnginesOfferPerLineCloning()
    {
        Assert.True(PerLineVoiceClone.CanBeOffered(new CosyVoice3CrispAsr(), "/videos/movie.mkv"));
        Assert.True(PerLineVoiceClone.CanBeOffered(new VoxCPM2CrispAsr(), "/videos/movie.mkv"));
        Assert.IsAssignableFrom<IPerLineCloneEngine>(new CosyVoice3CrispAsr());
        Assert.IsAssignableFrom<IPerLineCloneEngine>(new VoxCPM2CrispAsr());
    }

    [Fact]
    public void CosyVoice3_OrdinaryVoice_SendsNoVoiceField()
    {
        // The startup --voice/--ref-text flags carry an ordinary clone; a per-request absolute
        // path is a 400 and a bare stem is not a clone path (no .wav suffix).
        var payload = CosyVoice3CrispAsr.BuildSpeakPayload("hello", 1.0, null, null);

        Assert.False(payload.ContainsKey("voice"));
        Assert.False(payload.ContainsKey("ref_text"));
    }

    [Fact]
    public void CosyVoice3_PerLineClone_SendsFileNameWithWavSuffixAndTranscript()
    {
        // The backend takes the clone path only for a `voice` ending in .wav, and is conditioned
        // on the (transcript, speech) pair - so both travel with the request.
        var payload = CosyVoice3CrispAsr.BuildSpeakPayload("hello", 1.0, "se-per-line-line-0007.wav", "What the clip says.");

        Assert.Equal("se-per-line-line-0007.wav", payload["voice"]);
        Assert.Equal("What the clip says.", payload["ref_text"]);
        Assert.DoesNotContain('/', (string)payload["voice"]);
    }

    [Fact]
    public void VoxCpm2_OrdinaryVoice_SendsNoVoiceField()
    {
        var payload = VoxCPM2CrispAsr.BuildSpeakPayload("hello", 1.0, null);

        Assert.False(payload.ContainsKey("voice"));
    }

    [Fact]
    public void VoxCpm2_PerLineClone_SendsTheBareFileName()
    {
        var payload = VoxCPM2CrispAsr.BuildSpeakPayload("hello", 1.0, "se-per-line-line-0007.wav");

        Assert.Equal("se-per-line-line-0007.wav", payload["voice"]);
        Assert.False(payload.ContainsKey("ref_text"));
    }

    [Fact]
    public void TheVoicePointsAtTheStagedCopy()
    {
        var cosy = new CosyVoice3CrispAsr();
        var vox = new VoxCPM2CrispAsr();

        Assert.Equal("/voices/se-per-line-line-0003.wav",
            cosy.GetPerLineReferenceClip(new Voice(new CosyVoice3Voice("Ada", "/voices/se-per-line-line-0003.wav", "Hi."))));
        Assert.Equal("/voices/se-per-line-line-0003.wav",
            vox.GetPerLineReferenceClip(new Voice(new VoxCPM2Voice("Ada", "/voices/se-per-line-line-0003.wav"))));
        // A baked preset clones from nothing.
        Assert.Null(cosy.GetPerLineReferenceClip(new Voice(new CosyVoice3Voice("zero shot", "zero_shot"))));
        Assert.Null(vox.GetPerLineReferenceClip(new Voice(new CosyVoice3Voice("Ada", "/voices/x.wav", "Hi."))));
    }
}
