using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Zonos is not language-agnostic: CrispASR's zonos-tts backend phonemises the input with eSpeak
/// in whatever language it was started with, defaulting to en-us. Before #14433 SE never told it
/// one, so every non-English line was read with English G2P. These pin the language catalog and
/// the argument resolution the engine sends as the server's startup <c>-l</c> flag.
/// </summary>
public class ZonosLanguagesTests
{
    [Fact]
    public void Engine_HasLanguageParameter()
    {
        var engine = new ZonosTtsCrispAsr();

        Assert.True(engine.HasLanguageParameter);
    }

    [Fact]
    public void Languages_LeadWithEnglishUsAsTheBackendDefault()
    {
        // English (US) first so a combo falling back to its first entry reproduces the backend's
        // own default. There is no "Auto": Zonos cannot detect the language itself.
        var all = ZonosLanguages.All;

        Assert.Equal("English (US)", all[0].Name);
        Assert.Equal("en-us", all[0].Code);
        Assert.DoesNotContain(all, l => l.Name == "Auto");
        Assert.True(all.Length > 100);
    }

    [Fact]
    public void Languages_CodesAreUniqueAndSortedAfterTheDefault()
    {
        var all = ZonosLanguages.All;

        Assert.Equal(all.Length, all.Select(l => l.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        var names = all.Skip(1).Select(l => l.Name).ToList();
        Assert.Equal(names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList(), names);
    }

    [Theory]
    [InlineData("Czech", "cs")]
    [InlineData("German", "de")]
    [InlineData("French", "fr-fr")]
    [InlineData("Chinese (Mandarin)", "cmn")]
    [InlineData("Japanese", "ja")]
    [InlineData("English (US)", "en-us")]
    public void Languages_ResolveToEspeakCode(string displayName, string expectedArg)
    {
        // The codes are eSpeak voice names as listed in the GGUF's zonos.language_codes table,
        // not ISO 639-1 - "fr" or "zh" would be rejected by zonos_tts_set_language.
        var language = ZonosLanguages.All.Single(l => l.Name == displayName);

        Assert.Equal(expectedArg, ZonosLanguages.ResolveLanguageArg(language));
    }

    [Fact]
    public void Languages_ForeignEngineCodeIsDropped()
    {
        // A language object left over from another engine (e.g. Chatterbox's ISO 639-1 "fr")
        // must not reach the backend, which would log "unknown language code" and stay en-us.
        var foreign = new TtsLanguage("French", "fr");

        Assert.Equal(string.Empty, ZonosLanguages.ResolveLanguageArg(foreign));
    }

    [Fact]
    public void Languages_NullFallsBackToTheSavedPick()
    {
        // Cross-engine cast rows and the voice-test button pass null; the saved main-window
        // pick must still win (#13272 / #13470 pattern).
        var saved = Se.Settings.Video.TextToSpeech.ZonosTtsCrispAsrLanguage;
        try
        {
            Se.Settings.Video.TextToSpeech.ZonosTtsCrispAsrLanguage = "Czech";
            Assert.Equal("cs", ZonosLanguages.ResolveLanguageArg(null));

            Se.Settings.Video.TextToSpeech.ZonosTtsCrispAsrLanguage = string.Empty;
            Assert.Equal(string.Empty, ZonosLanguages.ResolveLanguageArg(null));
        }
        finally
        {
            Se.Settings.Video.TextToSpeech.ZonosTtsCrispAsrLanguage = saved;
        }
    }
}
