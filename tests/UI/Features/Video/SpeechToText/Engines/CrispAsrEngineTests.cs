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

    /// <summary>
    /// crispasr's parakeet backend is the transducer runtime and refuses a pure-CTC GGUF; the
    /// Vietnamese model (#14496) is one, so it has to go to fastconformer-ctc while the
    /// tdt_ctc hybrids (TDT head present) stay put.
    /// </summary>
    [Theory]
    [InlineData("parakeet-tdt-0.6b-v3-q4_k.gguf", "parakeet")]
    [InlineData("parakeet-tdt_ctc-110m-q8_0.gguf", "parakeet")]
    [InlineData("parakeet-tdt_ctc-1.1b.gguf", "parakeet")]
    [InlineData("parakeet-rnnt-0.6b-q4_k.gguf", "parakeet")]
    [InlineData("parakeet-ctc-0.6b-vi-q8_0.gguf", "fastconformer-ctc")]
    [InlineData("parakeet-ctc-0.6b-vi.gguf", "fastconformer-ctc")]
    public void Parakeet_GetBackendName_RoutesPureCtcModelsToTheCtcRuntime(string modelName, string expectedBackend)
    {
        var parakeet = new CrispAsrParakeet();
        var engine = new CrispAsrEngine();
        Assert.True(engine.TrySelectBackendChoice(WhisperChoice.CrispAsrParakeet));

        Assert.Equal(expectedBackend, parakeet.GetBackendName(modelName));
        Assert.Equal(expectedBackend, engine.GetBackendName(modelName));
    }

    [Fact]
    public void Parakeet_EveryCatalogModelRoutesByItsOwnName()
    {
        var parakeet = new CrispAsrParakeet();

        foreach (var model in parakeet.Models)
        {
            var isCtcOnly = model.Name.Contains("-ctc-") && !model.Name.Contains("tdt");
            Assert.Equal(isCtcOnly ? CrispAsrParakeet.CtcBackendName : "parakeet", parakeet.GetBackendName(model.Name));
        }
    }

    /// <summary>
    /// crispasr auto-enables FireRedPunc for fastconformer-ctc, which mangles Vietnamese spacing;
    /// the model punctuates by itself. The user's own punctuation flag always wins.
    /// </summary>
    [Theory]
    [InlineData("parakeet-ctc-0.6b-vi-q8_0.gguf", null, "--punc-model none")]
    [InlineData("parakeet-ctc-0.6b-vi-q8_0.gguf", "--max-len 50 --split-on-punct", "--punc-model none")]
    [InlineData("parakeet-ctc-0.6b-vi-q8_0.gguf", "--punc-model firered", "")]
    [InlineData("parakeet-ctc-0.6b-vi-q8_0.gguf", "--max-len 50 --no-punctuation", "")]
    [InlineData("parakeet-tdt-0.6b-v3-q4_k.gguf", null, "")]
    [InlineData("parakeet-tdt_ctc-1.1b-q8_0.gguf", null, "")]
    public void Parakeet_GetModelArguments_KeepsPunctuationRestorationOffForCtcModels(string modelName, string? userArgs, string expected)
    {
        var parakeet = new CrispAsrParakeet();
        var engine = new CrispAsrEngine();
        Assert.True(engine.TrySelectBackendChoice(WhisperChoice.CrispAsrParakeet));

        Assert.Equal(expected, parakeet.GetModelArguments(modelName, userArgs));
        Assert.Equal(expected, engine.GetModelArguments(modelName, userArgs));
    }

    [Fact]
    public void OtherBackends_DoNotOverridePerModelBackendOrArguments()
    {
        var engine = new CrispAsrEngine();

        foreach (var backend in engine.Backends.Where(p => p is not CrispAsrParakeet))
        {
            foreach (var model in backend.Models)
            {
                Assert.Equal(backend.BackendName, backend.GetBackendName(model.Name));
                Assert.Equal(string.Empty, backend.GetModelArguments(model.Name, null));
            }
        }
    }
}