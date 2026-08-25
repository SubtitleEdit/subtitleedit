using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Xunit;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// A Crisp ASR run that produced nothing looks identical whether the engine found no speech or was
/// killed before it wrote a byte, and SE told the user the same thing either way - which is all the
/// reporter in #14038 had to go on. The concrete case was the crispasr v0.8.29 GPU packages: built
/// with AVX-512 against a CI runner that had it (CrispASR #374), so every CPU without AVX-512 was
/// terminated inside ggml_backend_cpu_init while the CPU package ran fine on the same machine.
/// </summary>
public class CrispAsrCrashDiagnosticsTests
{
    [Fact]
    public void CleanExitIsNotACrash()
    {
        Assert.Null(SpeechToTextViewModel.DescribeCrispAsrCrash(0, "cuda"));
    }

    /// <summary>An unreadable exit code says nothing, so it must not claim a crash.</summary>
    [Fact]
    public void UnknownExitCodeIsNotACrash()
    {
        Assert.Null(SpeechToTextViewModel.DescribeCrispAsrCrash(null, "cuda"));
    }

    [Theory]
    [InlineData(SpeechToTextViewModel.StatusIllegalInstruction)]
    [InlineData(SpeechToTextViewModel.UnixSigill)]
    public void IllegalInstructionOnAGpuBuildNamesThePackageAndTheWayOut(int exitCode)
    {
        var message = SpeechToTextViewModel.DescribeCrispAsrCrash(exitCode, "cuda");

        Assert.NotNull(message);
        Assert.Contains("CPU instructions this computer does not have", message);
        Assert.Contains("\"cuda\" package", message);
        Assert.Contains("choose the CPU build", message);
    }

    /// <summary>Every GPU package is built by the same jobs, so all of them get the same advice.</summary>
    [Theory]
    [InlineData("cuda13")]
    [InlineData("vulkan")]
    [InlineData("hip")]
    public void EveryGpuVariantIsPointedAtTheCpuBuild(string variant)
    {
        var message = SpeechToTextViewModel.DescribeCrispAsrCrash(
            SpeechToTextViewModel.StatusIllegalInstruction, variant);

        Assert.NotNull(message);
        Assert.Contains($"\"{variant}\" package", message);
        Assert.Contains("choose the CPU build", message);
    }

    /// <summary>
    /// Already on the CPU build, or the sidecar could not be read: "use the CPU build" would be
    /// useless advice, so the fallback is the legacy build that targets the oldest CPUs.
    /// </summary>
    [Theory]
    [InlineData("cpu")]
    [InlineData(null)]
    public void NonGpuBuildIsPointedAtTheLegacyBuild(string? variant)
    {
        var message = SpeechToTextViewModel.DescribeCrispAsrCrash(
            SpeechToTextViewModel.StatusIllegalInstruction, variant);

        Assert.NotNull(message);
        Assert.Contains("CPU (legacy) build", message);
        Assert.DoesNotContain("package", message);
    }

    /// <summary>
    /// Some other crash is still worth reporting as a crash - it just gets no instruction-set
    /// advice, because that is not what went wrong.
    /// </summary>
    [Fact]
    public void OtherCrashesAreReportedWithoutInstructionSetAdvice()
    {
        var accessViolation = unchecked((int)0xC0000005);

        var message = SpeechToTextViewModel.DescribeCrispAsrCrash(accessViolation, "cuda");

        Assert.NotNull(message);
        Assert.Contains("crashed before producing any output", message);
        Assert.DoesNotContain("illegal instruction", message);
    }

    /// <summary>The raw code has to survive into the text: it is what a bug report needs.</summary>
    [Fact]
    public void TheExitCodeIsShownInBothDecimalAndHex()
    {
        var message = SpeechToTextViewModel.DescribeCrispAsrCrash(
            SpeechToTextViewModel.StatusIllegalInstruction, "cuda");

        Assert.NotNull(message);
        Assert.Contains("-1073741795", message);
        Assert.Contains("0xC000001D", message);
    }
}
