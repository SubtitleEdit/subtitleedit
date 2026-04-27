using System.Runtime.InteropServices;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.SpeechToText.Engines;

public class WhisperCppEngineTests
{
    [Fact]
    public void TrySelectBackendChoice_SelectsPersistedWindowsBackendChoice()
    {
        var engine = new WhisperCppEngine();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.False(engine.TrySelectBackendChoice(WhisperChoice.CppCuBlas));
            Assert.IsType<WhisperEngineCpp>(engine.SelectedBackend);
            return;
        }

        Assert.True(engine.TrySelectBackendChoice(WhisperChoice.CppCuBlas));
        Assert.IsType<WhisperEngineCppCuBlas>(engine.SelectedBackend);
        Assert.Equal(WhisperChoice.CppCuBlas, engine.Choice);

        Assert.True(engine.TrySelectBackendChoice(WhisperChoice.CppVulkan));
        Assert.IsType<WhisperEngineCppVulkan>(engine.SelectedBackend);
        Assert.Equal(WhisperChoice.CppVulkan, engine.Choice);
    }

    [Fact]
    public void Members_DelegateToSelectedBackend()
    {
        var originalCpp = Se.Settings.Tools.AudioToText.CommandLineParameterCpp;
        var originalVulkan = Se.Settings.Tools.AudioToText.CommandLineParameterCppVulkan;

        try
        {
            var engine = new WhisperCppEngine();
            var expectedBackend = engine.SelectedBackend;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.True(engine.TrySelectBackendChoice(WhisperChoice.CppVulkan));
                expectedBackend = engine.SelectedBackend;
            }

            engine.CommandLineParameter = "--unit-test-whisper-cpp";

            Assert.Equal(expectedBackend.Choice, engine.Choice);
            Assert.Equal(expectedBackend.Url, engine.Url);
            Assert.Equal(expectedBackend.Extension, engine.Extension);
            Assert.Equal(expectedBackend.UnpackSkipFolder, engine.UnpackSkipFolder);
            Assert.Equal(expectedBackend.CommandLineParameter, engine.CommandLineParameter);
            Assert.Equal(expectedBackend.Models.Select(p => p.Name), engine.Models.Select(p => p.Name));
            Assert.Equal(expectedBackend.Languages.Select(p => p.Code), engine.Languages.Select(p => p.Code));
        }
        finally
        {
            Se.Settings.Tools.AudioToText.CommandLineParameterCpp = originalCpp;
            Se.Settings.Tools.AudioToText.CommandLineParameterCppVulkan = originalVulkan;
        }
    }
}