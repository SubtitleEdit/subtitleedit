using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Supertonic-3 is the one CrispASR-backed engine that does not clone: ten preset voices baked
/// into the GGUF, 31 languages, everything chosen per request. The backend is sticky - a request
/// without <c>voice</c> or <c>language</c> keeps the previous request's value, and an unknown
/// value is answered by silently keeping the current one - so these pin that SE always sends
/// both, and only ever values the backend knows.
/// </summary>
public class SupertonicCrispAsrTests
{
    [Fact]
    public void Engine_IsAFixedVoiceEngineWithALanguagePick()
    {
        var engine = new SupertonicCrispAsr();

        Assert.True(engine.HasLanguageParameter);
        Assert.False(engine.HasModel);
        Assert.False(engine.SupportsVoiceCloning);
        Assert.False(engine.SupportsPerLineVoiceCloning);
        Assert.False(engine.ImportVoice("/voices/ada.wav"));
    }

    [Fact]
    public void Catalog_OffersItButNotAsACloningEngine()
    {
        Assert.Contains(TtsEngineCatalog.CreateAll(null!), e => e is SupertonicCrispAsr);
        Assert.DoesNotContain(TtsEngineCatalog.CreateVoiceCloningEngines(), e => e is SupertonicCrispAsr);
    }

    [Fact]
    public async Task Voices_AreTheTenPresetsWhateverTheLanguage()
    {
        var engine = new SupertonicCrispAsr();

        var voices = await engine.GetVoices("da");

        Assert.Equal(
            new[] { "F1", "F2", "F3", "F4", "F5", "M1", "M2", "M3", "M4", "M5" },
            voices.Select(v => ((SupertonicVoice)v.EngineVoice!).Voice).ToArray());
        Assert.Equal("Female 1", voices[0].Name);
        Assert.Equal("Male 5", voices[9].Name);
        Assert.Equal(voices.Length, voices.Select(v => v.Name).Distinct().Count());
    }

    [Theory]
    [InlineData("F3", "F3")]
    [InlineData("m2", "M2")]
    [InlineData("Z9", SupertonicCrispAsr.DefaultVoice)]
    [InlineData("", SupertonicCrispAsr.DefaultVoice)]
    public void ResolveVoiceId_OnlyPassesOnPresetsTheBackendKnows(string id, string expected)
    {
        // The backend answers an unknown name by keeping the previous request's voice, which
        // would make a line's speaker depend on the line before it.
        Assert.Equal(expected, SupertonicCrispAsr.ResolveVoiceId(new Voice(new SupertonicVoice(id))));
    }

    [Fact]
    public void ResolveVoiceId_ForeignEngineVoiceFallsBackToTheDefault()
    {
        Assert.Equal(SupertonicCrispAsr.DefaultVoice, SupertonicCrispAsr.ResolveVoiceId(new Voice(new KokoroVoice("af_maple"))));
        Assert.Equal(SupertonicCrispAsr.DefaultVoice, SupertonicCrispAsr.ResolveVoiceId(null));
    }

    [Fact]
    public void Payload_AlwaysCarriesVoiceAndLanguage()
    {
        var payload = SupertonicCrispAsr.BuildSpeechPayload("Hej med dig.", "F2", "da", 1.2);

        Assert.Equal("Hej med dig.", payload["input"]);
        Assert.Equal("wav", payload["response_format"]);
        Assert.Equal("F2", payload["voice"]);
        Assert.Equal("da", payload["language"]);
        Assert.Equal(1.2, (double)payload["speed"]);
    }

    [Theory]
    [InlineData(0.0, 0.25)]
    [InlineData(9.0, 4.0)]
    public void Payload_ClampsSpeedToTheServersRange(double speed, double expected)
    {
        // The server rejects a speed outside 0.25-4.0 with HTTP 400 rather than clamping it.
        var payload = SupertonicCrispAsr.BuildSpeechPayload("Hi", "M1", "en", speed);

        Assert.Equal(expected, (double)payload["speed"]);
    }

    [Fact]
    public void Languages_LeadWithEnglishAsTheBackendDefault()
    {
        var all = SupertonicLanguages.All;

        Assert.Equal("English", all[0].Name);
        Assert.Equal("en", all[0].Code);
        Assert.DoesNotContain(all, l => l.Name == "Auto");
        Assert.Equal(31, all.Length);
    }

    [Fact]
    public void Languages_AreExactlyTheCodesTheBackendAccepts()
    {
        // Verbatim from the v0.8.34 backend's own "language not supported" message.
        var backend = "en ko ja ar bg cs da de el es et fi fr hi hr hu id it lt lv nl pl pt ro ru sk sl sv tr uk vi"
            .Split(' ');

        Assert.Equal(
            backend.OrderBy(c => c, StringComparer.Ordinal).ToArray(),
            SupertonicLanguages.All.Select(l => l.Code).OrderBy(c => c, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void Languages_AreSortedAfterTheDefault()
    {
        var names = SupertonicLanguages.All.Skip(1).Select(l => l.Name).ToList();

        Assert.Equal(names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList(), names);
    }

    [Fact]
    public void Languages_ForeignEngineCodeFallsBackToEnglishRatherThanNothing()
    {
        // Zonos' "en-us" or Chatterbox's "zh" are not Supertonic codes. An empty field is not an
        // option here: the backend would keep the previous line's language.
        Assert.Equal("en", SupertonicLanguages.ResolveLanguageArg(new TtsLanguage("English (US)", "en-us")));
        Assert.Equal("en", SupertonicLanguages.ResolveLanguageArg(new TtsLanguage("Chinese", "zh")));
        Assert.Equal("da", SupertonicLanguages.ResolveLanguageArg(new TtsLanguage("Danish", "da")));
    }

    [Fact]
    public void Languages_NullFallsBackToTheSavedPick()
    {
        // Cross-engine cast rows and the voice-test button pass null; the saved main-window
        // pick must still win (#13272 / #13470 pattern).
        var saved = Se.Settings.Video.TextToSpeech.SupertonicCrispAsrLanguage;
        try
        {
            Se.Settings.Video.TextToSpeech.SupertonicCrispAsrLanguage = "Danish";
            Assert.Equal("da", SupertonicLanguages.ResolveLanguageArg(null));

            Se.Settings.Video.TextToSpeech.SupertonicCrispAsrLanguage = string.Empty;
            Assert.Equal("en", SupertonicLanguages.ResolveLanguageArg(null));
        }
        finally
        {
            Se.Settings.Video.TextToSpeech.SupertonicCrispAsrLanguage = saved;
        }
    }
}
