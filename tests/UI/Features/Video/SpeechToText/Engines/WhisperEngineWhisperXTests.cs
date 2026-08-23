using System.Linq;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace UITests.Features.Video.SpeechToText.Engines;

public class WhisperEngineWhisperXTests
{
    [Fact]
    public void FactoryCreatesWhisperXEngine()
    {
        var engine = WhisperEngineFactory.MakeEngineFromStaticName(WhisperEngineWhisperX.StaticName);

        Assert.IsType<WhisperEngineWhisperX>(engine);
        Assert.Equal(WhisperChoice.WhisperX, engine.Choice);
        Assert.True(engine.CanBeDownloaded());
    }

    [Fact]
    public void LanguageCatalogIncludesLanguagesBeyondTheOriginalWhisperXSubset()
    {
        var engine = new WhisperEngineWhisperX();
        var languageCodes = engine.Languages.Select(p => p.Code).ToHashSet();

        Assert.Contains("tr", languageCodes);
        Assert.Contains("ar", languageCodes);
        Assert.Contains("ru", languageCodes);
        Assert.Contains("fa", languageCodes);
        Assert.True(languageCodes.Count > 50);
    }

    [Fact]
    public void ParametersCanSelectReliableMacCpuMode()
    {
        var engine = new WhisperEngineWhisperX();
        var original = engine.CommandLineParameter;

        try
        {
            engine.CommandLineParameter = "--device cpu --compute_type int8";
            Assert.Contains("--device cpu", engine.CommandLineParameter);
            Assert.Contains("--compute_type int8", engine.CommandLineParameter);
        }
        finally
        {
            engine.CommandLineParameter = original;
        }
    }
}
