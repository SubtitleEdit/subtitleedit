using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The first-clone consent gate: when the dialog is owed, what accepting and declining leave
/// behind, and which engines and voices count as cloning at all.
/// </summary>
[Collection(TtsSettingsCollection.Name)]
public class VoiceCloningConsentTests
{
    [Fact]
    public void IsAccepted_OnAFreshInstall_IsFalse()
    {
        // AcceptVoiceCloning has defaulted to true since before this dialog existed, so it cannot
        // be the thing that decides whether to ask - the version stamp is. Without this, nobody
        // would ever see the dialog.
        using var _ = new VoiceCloningConsentScope(accepted: true, consent: string.Empty);

        Assert.False(VoiceCloningConsent.IsAccepted);
    }

    [Fact]
    public void Accept_StampsTheVersionAndTurnsCloningOn()
    {
        using var _ = new VoiceCloningConsentScope(accepted: false, consent: string.Empty);

        VoiceCloningConsent.Accept();

        Assert.True(Se.Settings.Video.TextToSpeech.AcceptVoiceCloning);
        Assert.Equal(VoiceCloningConsent.ConsentVersion, Se.Settings.Video.TextToSpeech.VoiceCloningConsent);
        Assert.True(VoiceCloningConsent.IsAccepted);
    }

    [Fact]
    public void Decline_LeavesTheVersionUnstampedSoTheQuestionComesBack()
    {
        using var _ = new VoiceCloningConsentScope(accepted: true, consent: VoiceCloningConsent.ConsentVersion);

        VoiceCloningConsent.Decline();

        Assert.Equal(string.Empty, Se.Settings.Video.TextToSpeech.VoiceCloningConsent);
        Assert.False(VoiceCloningConsent.IsAccepted);
    }

    [Fact]
    public void Decline_DoesNotSwitchCloningOff()
    {
        // AcceptVoiceCloning also governs the spoken AI disclaimer CrispASR puts in front of ALL
        // its output, cloned or not. Clearing it on a closed dialog would make every later
        // generation - on voices that clone nothing - announce itself out loud.
        using var _ = new VoiceCloningConsentScope(accepted: true, consent: string.Empty);

        VoiceCloningConsent.Decline();

        Assert.True(Se.Settings.Video.TextToSpeech.AcceptVoiceCloning);
    }

    [Fact]
    public void IsAccepted_WithAnOlderVersionStamp_IsFalse()
    {
        // Bumping ConsentVersion is how changed terms get re-read.
        using var _ = new VoiceCloningConsentScope(accepted: true, consent: "voice-clone-consent-1999-01");

        Assert.False(VoiceCloningConsent.IsAccepted);
    }

    [Fact]
    public void IsAccepted_WhenCloningIsSwitchedOff_IsFalse()
    {
        // Turning AcceptVoiceCloning off is the only revoke there is; it has to bring the dialog
        // back rather than leave a stamped-but-disabled state no UI can recover from.
        using var _ = new VoiceCloningConsentScope(accepted: false, consent: VoiceCloningConsent.ConsentVersion);

        Assert.False(VoiceCloningConsent.IsAccepted);
    }

    [Fact]
    public void RequiresConsent_ForPiper_IsFalse()
    {
        // Piper's import takes a trained .onnx model - nobody's voice is being copied, and asking
        // there would only train users to click the dialog away.
        Assert.False(VoiceCloningConsent.RequiresConsent(new Piper(null!)));
    }

    [Fact]
    public void RequiresConsent_ForACloningEngine_IsTrue()
    {
        Assert.True(VoiceCloningConsent.RequiresConsent(new ChatterboxTtsCpp()));
    }

    [Fact]
    public void RequiresConsent_WithNoEngine_IsFalse()
    {
        Assert.False(VoiceCloningConsent.RequiresConsent(null));
    }

    [Theory]
    [MemberData(nameof(CloneVoices))]
    public void IsCloneVoice_WithAReferenceWav_IsTrue(object engineVoice)
    {
        Assert.True(VoiceCloningConsent.IsCloneVoice(new Voice(engineVoice)));
    }

    [Theory]
    [MemberData(nameof(PresetVoices))]
    public void IsCloneVoice_WithNoReferenceWav_IsFalse(object engineVoice)
    {
        // Same types, empty FilePath: a baked-in speaker, a CosyVoice3 preset, a Qwen3 designed
        // voice. Gating these would ask on every generate for engines that clone nothing.
        Assert.False(VoiceCloningConsent.IsCloneVoice(new Voice(engineVoice)));
    }

    [Fact]
    public void IsCloneVoice_WithAVoiceFromANonCloningEngine_IsFalse()
    {
        Assert.False(VoiceCloningConsent.IsCloneVoice(new Voice(new PiperVoice("alan", "en_GB", "medium", "alan.onnx", "alan.onnx.json"))));
        Assert.False(VoiceCloningConsent.IsCloneVoice(new Voice(new KokoroVoice("af_maple"))));
    }

    [Fact]
    public void IsCloneVoice_WithNoVoice_IsFalse()
    {
        Assert.False(VoiceCloningConsent.IsCloneVoice(null));
    }

    public static TheoryData<object> CloneVoices() => new()
    {
        new ChatterboxVoice("Ada", "/voices/ada.wav"),
        new CosyVoice3Voice("Ada", "/voices/ada.wav", string.Empty),
        new F5TtsVoice("Ada", "/voices/ada.wav"),
        new IndexTtsVoice("Ada", "/voices/ada.wav"),
        new MossTtsVoice("Ada", "/voices/ada.wav"),
        new OmniVoice("Ada", "/voices/ada.wav"),
        new OmniVoiceCrispAsrVoice("Ada", "/voices/ada.wav"),
        new Qwen3TtsVoice("Ada", "/voices/ada.wav"),
        new VibeVoice("Ada", "/voices/ada.wav"),
        new VoxCPM2Voice("Ada", "/voices/ada.wav"),
        new ZonosTtsVoice("Ada", "/voices/ada.wav"),
    };

    public static TheoryData<object> PresetVoices() => new()
    {
        new ChatterboxVoice(),
        new CosyVoice3Voice(),
        new F5TtsVoice(),
        new IndexTtsVoice(),
        new MossTtsVoice(),
        new OmniVoice(),
        new OmniVoiceCrispAsrVoice(),
        new Qwen3TtsVoice(),
        new VibeVoice(),
        new VoxCPM2Voice(),
        new ZonosTtsVoice(),
    };
}

/// <summary>
/// Restores both halves of the consent state after a test flips them.
/// </summary>
internal sealed class VoiceCloningConsentScope : IDisposable
{
    private readonly bool _originalAccepted;
    private readonly string _originalConsent;

    public VoiceCloningConsentScope(bool accepted, string consent)
    {
        _originalAccepted = Se.Settings.Video.TextToSpeech.AcceptVoiceCloning;
        _originalConsent = Se.Settings.Video.TextToSpeech.VoiceCloningConsent;
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning = accepted;
        Se.Settings.Video.TextToSpeech.VoiceCloningConsent = consent;
    }

    public void Dispose()
    {
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning = _originalAccepted;
        Se.Settings.Video.TextToSpeech.VoiceCloningConsent = _originalConsent;
    }
}
