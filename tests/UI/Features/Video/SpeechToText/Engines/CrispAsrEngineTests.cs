using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.SpeechToText.Engines;

public class CrispAsrEngineTests
{
    [Theory]
    [InlineData(WhisperChoice.CrispAsrOmni, typeof(CrispAsrOmni), "omniasr")]
    [InlineData(WhisperChoice.CrispAsrQwen3, typeof(CrispAsrQwen3), "qwen3")]
    [InlineData(WhisperChoice.CrispAsrKyutai, typeof(CrispAsrKyutai), "kyutai-stt")]
    [InlineData(WhisperChoice.CrispAsrMega, typeof(CrispAsrMega), "mega-asr")]
    [InlineData(WhisperChoice.CrispAsrFunAsrNano, typeof(CrispAsrFunAsrNano), "funasr")]
    [InlineData(WhisperChoice.CrispAsrGigaAm, typeof(CrispAsrGigaAm), "gigaam")]
    [InlineData(WhisperChoice.CrispAsrVoxtral, typeof(CrispAsrVoxtral), "voxtral")]
    public void TrySelectBackendChoice_SelectsPersistedCrispBackendChoice(string choice, Type backendType, string backendName)
    {
        var engine = new CrispAsrEngine();

        Assert.True(engine.TrySelectBackendChoice(choice));

        Assert.IsType(backendType, engine.SelectedBackend);
        Assert.Equal(choice, engine.Choice);
        Assert.Equal(backendName, engine.BackendName);
    }

    [Fact]
    public void Members_DelegateToSelectedBackend()
    {
        var originalOmni = Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrOmni;

        try
        {
            var engine = new CrispAsrEngine();
            Assert.True(engine.TrySelectBackendChoice(WhisperChoice.CrispAsrOmni));

            var expectedBackend = engine.SelectedBackend;
            engine.CommandLineParameter = "--unit-test-crisp-asr";

            Assert.Equal(expectedBackend.Choice, engine.Choice);
            Assert.Equal(expectedBackend.Url, engine.Url);
            Assert.Equal(expectedBackend.BackendName, engine.BackendName);
            Assert.Equal(expectedBackend.DefaultLanguage, engine.DefaultLanguage);
            Assert.Equal(expectedBackend.IncludeLanguage, engine.IncludeLanguage);
            Assert.Equal(expectedBackend.Extension, engine.Extension);
            Assert.Equal(expectedBackend.UnpackSkipFolder, engine.UnpackSkipFolder);
            Assert.Equal(expectedBackend.CommandLineParameter, engine.CommandLineParameter);
            Assert.Equal(expectedBackend.Models.Select(p => p.Name), engine.Models.Select(p => p.Name));
            Assert.Equal(expectedBackend.Languages.Select(p => p.Code), engine.Languages.Select(p => p.Code));
        }
        finally
        {
            Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrOmni = originalOmni;
        }
    }

    /// <summary>
    /// crispasr's <c>-l auto</c> is a backend-agnostic flag (issue #14483): non-native backends
    /// pick a language-detect provider via <c>--lid-backend</c> (whisper/silero/probe/...), so
    /// every multi-language backend can offer "Auto detect" in its language dropdown. The lone
    /// documented exception is GigaAM, which is Russian-only and for which CrispASR explicitly
    /// skips language detection - see the comment on <see cref="CrispAsrGigaAm"/>.
    /// </summary>
    [Fact]
    public void Languages_OfferAutoDetectAsFirstEntry_ForEveryMultiLanguageBackend()
    {
        var engine = new CrispAsrEngine();

        foreach (var backend in engine.Backends)
        {
            if (backend is CrispAsrGigaAm || !backend.IncludeLanguage || backend.Languages.Count == 0)
            {
                continue;
            }

            var first = backend.Languages[0];
            Assert.True(first.Code == "auto", $"{backend.Name} is missing a leading \"auto\" language entry.");
            Assert.Equal("Auto Detect", first.Name);
        }
    }
}