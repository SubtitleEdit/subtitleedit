using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// #17216: the Linux CUDA build of Crisp ASR needs the system CUDA runtime, and without it ggml's
/// dlopen of libggml-cuda.so fails silently - the run finishes on the CPU with a clean exit.
/// </summary>
public class CrispAsrGpuFallbackTests
{
    /// <summary>The start of the log from the issue: CUDA build, CPU backend only.</summary>
    private static readonly string[] IssueLog =
    [
        "crispasr 0.8.50 (git 5c96e552, Release) [backends: cpu,cuda]",
        "load_backend: loaded CPU backend from /home/u/.config/Subtitle Edit/CrispASR/libggml-cpu-haswell.so",
        "cohere: backend: CPU (CPU-only)",
    ];

    [Fact]
    public void CudaBuildThatOnlyLoadedCpuIsAFallback()
    {
        Assert.True(CrispAsrGpuFallback.IsCudaCpuFallback(IssueLog));
    }

    [Fact]
    public void LoadedCudaBackendIsNotAFallback()
    {
        var log = IssueLog.Append("load_backend: loaded CUDA backend from /x/libggml-cuda.so");
        Assert.False(CrispAsrGpuFallback.IsCudaCpuFallback(log));
    }

    [Fact]
    public void CudaInitLineIsNotAFallback()
    {
        var log = IssueLog.Append("ggml_cuda_init: found 1 CUDA devices:");
        Assert.False(CrispAsrGpuFallback.IsCudaCpuFallback(log));
    }

    [Fact]
    public void CpuBuildIsNeverFlagged()
    {
        var log = new[]
        {
            "crispasr 0.8.55 (git abc, Release) [backends: cpu]",
            "load_backend: loaded CPU backend from /x/libggml-cpu-haswell.so",
        };
        Assert.False(CrispAsrGpuFallback.IsCudaCpuFallback(log));
    }

    /// <summary>Without the dynamic-backend lines there is nothing to judge by.</summary>
    [Fact]
    public void NoLoadBackendLinesIsNotFlagged()
    {
        Assert.False(CrispAsrGpuFallback.IsCudaCpuFallback(["crispasr 0.8.55 [backends: cpu,cuda]"]));
    }

    [Fact]
    public void ParsesNotFoundLinesFromLdd()
    {
        const string ldd =
            "\tlinux-vdso.so.1 (0x00007ffd)\n" +
            "\tlibcudart.so.12 => not found\n" +
            "\tlibcublas.so.12 => not found\n" +
            "\tlibcuda.so.1 => /usr/lib/libcuda.so.1 (0x00007f00)\n" +
            "\tlibcublas.so.12 => not found\n";

        Assert.Equal(["libcudart.so.12", "libcublas.so.12"], CrispAsrGpuFallback.ParseMissingLibraries(ldd));
    }

    [Fact]
    public void EmptyLddOutputHasNoMissingLibraries()
    {
        Assert.Empty(CrispAsrGpuFallback.ParseMissingLibraries(null));
        Assert.Empty(CrispAsrGpuFallback.ParseMissingLibraries(string.Empty));
    }

    [Theory]
    [InlineData("libcudart.so.12", "CUDA 12 toolkit")]
    [InlineData("libcudart.so.13", "CUDA 13 toolkit")]
    public void DescribeNamesLibrariesAndToolkitVersion(string library, string expected)
    {
        var message = CrispAsrGpuFallback.Describe([library]);

        Assert.Contains(library, message);
        Assert.Contains(expected, message);
        Assert.Contains("Vulkan", message);
    }

    [Fact]
    public void DescribeWithoutMissingLibrariesPointsAtTheDriver()
    {
        var message = CrispAsrGpuFallback.Describe([]);

        Assert.Contains("nvidia-smi", message);
        Assert.DoesNotContain("Missing libraries", message);
    }
}
