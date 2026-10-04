using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Xunit;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// The "VAD" choice for Crisp ASR (#15563). CrispASR tells its detectors apart by the model's file
/// name, so those names are part of the contract.
/// </summary>
public class CrispAsrVadModelTests
{
    [Fact]
    public void AutoMeansSilero()
    {
        Assert.Equal(CrispAsrVadModel.Silero, CrispAsrVadModel.GetEffective(CrispAsrVadModel.Automatic, new CrispAsrParakeet()).Choice);
    }

    [Fact]
    public void AChosenVadIsUsedAsIs()
    {
        Assert.Equal(CrispAsrVadModel.FireRed, CrispAsrVadModel.GetEffective(CrispAsrVadModel.FireRed, new CrispAsrCohere()).Choice);
    }

    /// <summary>Index-Echo loads the VAD model itself and can only read Silero.</summary>
    [Fact]
    public void IndexEchoAlwaysUsesSilero()
    {
        Assert.Equal(CrispAsrVadModel.Silero, CrispAsrVadModel.GetEffective(CrispAsrVadModel.FireRed, new CrispAsrIndexEcho()).Choice);
    }

    [Fact]
    public void AnUnknownChoiceIsAuto()
    {
        Assert.Equal(CrispAsrVadModel.Automatic, CrispAsrVadModel.Get("ten-vad").Choice);
        Assert.Equal(CrispAsrVadModel.Automatic, CrispAsrVadModel.Get(null).Choice);
    }

    [Fact]
    public void ModelFileNamesAreTheOnesCrispAsrRecognises()
    {
        var fireRed = CrispAsrVadModel.Get(CrispAsrVadModel.FireRed).FileName!;
        Assert.Contains("firered", fireRed);
        Assert.Contains("vad", fireRed);
    }

    [Fact]
    public void WebRtcNeedsNoModelFile()
    {
        var webRtc = CrispAsrVadModel.Get(CrispAsrVadModel.WebRtc);
        Assert.False(webRtc.NeedsDownload);
        Assert.Equal("webrtc", CrispAsrVadModel.GetModelPath(webRtc, new CrispAsrParakeet()));
        Assert.Equal("--vad --vad-model webrtc", CrispAsrVadModel.BuildArguments("webrtc"));
    }

    [Fact]
    public void AModelPathIsQuoted()
    {
        Assert.Equal("--vad --vad-model \"/a b/firered-vad.gguf\"", CrispAsrVadModel.BuildArguments("/a b/firered-vad.gguf"));
    }

    [Fact]
    public void FindMissingModel_FireRedAtItsDownloadPath()
    {
        var engine = new CrispAsrParakeet();
        var fireRed = CrispAsrVadModel.Get(CrispAsrVadModel.FireRed);
        var path = engine.GetModelForCmdLine(fireRed.FileName!);
        var args = "-t 8 " + CrispAsrVadModel.BuildArguments(path);

        var missing = CrispAsrVadModel.FindMissingModelInArguments(args, engine);

        if (File.Exists(path))
        {
            Assert.Null(missing);
        }
        else
        {
            Assert.Same(fireRed, missing);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("--vad")]
    [InlineData("--vad --vad-model webrtc")]
    [InlineData("--vad --vad-model \"/somewhere/else/firered-vad.gguf\"")]
    [InlineData("--vad -vm /x/ggml-silero-v5.1.2.bin")]
    public void FindMissingModel_IgnoresOtherVadArguments(string args)
    {
        Assert.Null(CrispAsrVadModel.FindMissingModelInArguments(args, new CrispAsrParakeet()));
    }
}
