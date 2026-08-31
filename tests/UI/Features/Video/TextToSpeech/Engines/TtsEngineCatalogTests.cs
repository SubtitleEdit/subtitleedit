using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The catalog exists so the TTS window's engine list and the waveform's "Clone voice to" menu
/// cannot drift apart. These tests are that promise: the cloning list is exactly the cloning half
/// of the full list, and an engine disabled in one place is not still offered in the other.
/// </summary>
public class TtsEngineCatalogTests
{
    [Fact]
    public void EveryEngineInTheCloningListCanActuallyClone()
    {
        foreach (var engine in TtsEngineCatalog.CreateVoiceCloningEngines())
        {
            Assert.True(engine.SupportsVoiceCloning, $"{engine.Name} is offered as a cloning engine but says it cannot clone.");
        }
    }

    [Fact]
    public void TheCloningListIsExactlyTheCloningEnginesOfTheFullList()
    {
        // Both directions matter: an engine missing here would be unreachable from the waveform,
        // and one that is only here (e.g. left behind when it was commented out of CreateAll)
        // would be offered for cloning while the TTS window cannot select it afterwards.
        var fromAll = TtsEngineCatalog.CreateAll(null!)
            .Where(e => e.SupportsVoiceCloning)
            .Select(e => e.Name)
            .ToList();
        var cloningOnly = TtsEngineCatalog.CreateVoiceCloningEngines()
            .Select(e => e.Name)
            .ToList();

        Assert.Equal(fromAll, cloningOnly);
    }

    [Fact]
    public void PiperIsNotACloningEngine()
    {
        // Piper's import takes a trained .onnx model rather than a recording of someone speaking,
        // so it must stay out of the clone menu (and out of the consent gate).
        Assert.False(new Piper(null!).SupportsVoiceCloning);
        Assert.DoesNotContain(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is Piper);
    }

    [Fact]
    public void TheHiddenEnginesStayHidden()
    {
        var engines = TtsEngineCatalog.CreateAll(null!);

        // F5-TTS can clone but has no GPU backend, so it is disabled on purpose - offering it
        // from the waveform menu would be re-enabling it by accident.
        Assert.DoesNotContain(engines, e => e is F5TtsCrispAsr);
        Assert.DoesNotContain(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is F5TtsCrispAsr);
    }

    [Fact]
    public void VibeVoiceIsOffered()
    {
        // Hidden while its output quality was judged unusable on the CrispASR build of the time;
        // re-checked on v0.8.31 and re-enabled. Pinned so a stray comment-out is caught: the
        // engine is fully wired (DI, settings dialog, installer, status dots) and the only thing
        // that ever gated it was this one line in the catalog.
        Assert.Contains(TtsEngineCatalog.CreateAll(null!), e => e is VibeVoiceCrispAsr);
        Assert.Contains(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is VibeVoiceCrispAsr);
    }

    [Fact]
    public void EngineNamesAreUnique()
    {
        // The clone menu shows one sub item per engine, labelled by name - two engines sharing a
        // name would give the user two identical looking items.
        var names = TtsEngineCatalog.CreateAll(null!).Select(e => e.Name).ToList();

        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
