using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// qwen3-asr.cpp v0.1.10's Linux packages were built for the CI runner's CPU, so on most other
/// CPUs the engine died at once with exit code 132 and SE suggested trying a different model
/// (#16593). An illegal instruction is a build/CPU mismatch and must say so.
/// </summary>
public class Qwen3AsrCrashDiagnosticsTests
{
    private const string Url = "https://github.com/niksedk/qwen3-asr.cpp";

    [Fact]
    public void CleanOrUnknownExitIsNotACrash()
    {
        Assert.Null(SpeechToTextViewModel.DescribeQwen3AsrCrash(0, false, Url));
        Assert.Null(SpeechToTextViewModel.DescribeQwen3AsrCrash(null, true, Url));
    }

    [Theory]
    [InlineData(SpeechToTextViewModel.StatusIllegalInstruction, false)]
    [InlineData(SpeechToTextViewModel.UnixSigill, false)]
    [InlineData(SpeechToTextViewModel.UnixSigill, true)]
    public void IllegalInstructionPointsAtTheEngineDownloadNotTheModel(int exitCode, bool isVulkan)
    {
        var message = SpeechToTextViewModel.DescribeQwen3AsrCrash(exitCode, isVulkan, Url);

        Assert.NotNull(message);
        Assert.Contains("CPU instructions this computer does not have", message);
        Assert.Contains("Download the Qwen3 ASR engine again", message);
        Assert.DoesNotContain("different model", message);
    }

    [Fact]
    public void OtherCrashKeepsTheGenericAdvice()
    {
        Assert.Contains("different model", SpeechToTextViewModel.DescribeQwen3AsrCrash(-1073741819, false, Url));
        Assert.Contains("choose CPU", SpeechToTextViewModel.DescribeQwen3AsrCrash(-1073741819, true, Url));
    }
}
