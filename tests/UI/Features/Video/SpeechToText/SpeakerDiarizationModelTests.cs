using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText;

public class SpeakerDiarizationModelTests
{
    [Fact]
    public void Arguments_RunSortformerWithTheGivenModel()
    {
        var args = SpeakerDiarizationModel.BuildArguments("/models dir/Nemotron-3-Diarization.q8_0.gguf");

        Assert.Equal("--diarize --diarize-method sortformer --diarize-model \"/models dir/Nemotron-3-Diarization.q8_0.gguf\"", args);
    }

    [Theory]
    [InlineData("0.8.37", true)]
    [InlineData("0.8.38", true)]
    [InlineData("0.9.0", true)]
    [InlineData("v0.8.37", true)]
    [InlineData("0.8.37-dirty", true)]
    [InlineData("0.8.36", false)]
    [InlineData("0.8.4", false)]
    [InlineData("0.7.99", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("unknown", true)]
    public void OnlyCrispAsrKnownToBeTooOldIsRefused(string? version, bool expected)
    {
        Assert.Equal(expected, SpeakerDiarizationModel.IsSupportedBy(version));
    }

    [Fact]
    public void Download_IsTheQ8FileFromNvidia()
    {
        var model = SpeakerDiarizationModel.ToWhisperModel();

        Assert.Equal("Nemotron-3-Diarization.q8_0.gguf", model.Name);
        Assert.Equal("https://huggingface.co/nvidia/Nemotron-3-Diarization/resolve/main/Nemotron-3-Diarization.q8_0.gguf", Assert.Single(model.Urls));
    }

    [Fact]
    public void DetectSpeakers_OnlyRunsOnCrispAsrBackendsThatDoNotLabelSpeakersThemselves()
    {
        Assert.True(SpeechToTextViewModel.ShouldDetectSpeakers(true, new CrispAsrParakeet()));
        Assert.False(SpeechToTextViewModel.ShouldDetectSpeakers(false, new CrispAsrParakeet()));
        Assert.False(SpeechToTextViewModel.ShouldDetectSpeakers(true, new CrispAsrMossDiarize()));
        Assert.False(SpeechToTextViewModel.ShouldDetectSpeakers(true, new WhisperEngineCpp()));
    }
}
