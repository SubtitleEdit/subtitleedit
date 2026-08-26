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
    public void ModelsAreAlwaysReportedInstalled_WhisperXDownloadsThemItself()
    {
        // Reporting "not installed" made SE offer its own model download into a folder
        // whisperx never reads, re-prompting on every run - the engine owns its models.
        var engine = new WhisperEngineWhisperX();

        Assert.All(engine.Models, m => Assert.True(engine.IsModelInstalled(m)));
    }

    [Fact]
    public void ModelListExcludesNamesWhisperXCannotResolve()
    {
        // The NbAiLab "*.nb" names only work when SE downloads the model and passes a local
        // folder; whisperx gets the bare name, which is not a size or a Hugging Face repo id.
        var engine = new WhisperEngineWhisperX();

        Assert.DoesNotContain(engine.Models, m => m.Name.EndsWith(".nb"));
        Assert.Contains(engine.Models, m => m.Name == "large-v3");
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

    [Fact]
    public void ExecutableLivesDirectlyInsideTheWhisperXFolder()
    {
        // The standalone build's zip is unpacked flat (Unpack(dir, string.Empty)), so the
        // executable and its "_internal" PyInstaller payload land directly in the engine
        // folder - same layout as WhisperEngineCTranslate2, not a nested subfolder.
        var engine = new WhisperEngineWhisperX();

        var executable = engine.GetExecutable();
        var folder = engine.GetAndCreateWhisperFolder();

        Assert.Equal(folder, System.IO.Path.GetDirectoryName(executable));
    }
}
