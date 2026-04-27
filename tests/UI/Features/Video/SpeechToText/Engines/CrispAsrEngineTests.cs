using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.SpeechToText.Engines;

public class CrispAsrEngineTests
{
    [Theory]
    [InlineData(WhisperChoice.CrispAsrOmni, typeof(CrispAsrOmni), "omniasr")]
    [InlineData(WhisperChoice.CrispAsrQwen3, typeof(CrispAsrQwen3), "qwen3")]
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
}