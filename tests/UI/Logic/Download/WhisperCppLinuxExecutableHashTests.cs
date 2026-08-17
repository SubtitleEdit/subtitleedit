using System;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace UITests.Logic.Download;

/// <summary>
/// Every Linux whisper.cpp build from v1.8.4 to v1.9.1 shipped whisper-cli without the
/// libwhisper.so.1 it links against, so the engine could not start at all (issue #13680). The
/// rebuilt archives from whispercpp-191-r2 on fix that, and these tests pin the path by which an
/// affected user finds out: an install with no .installed.sha256 sidecar is identified by hashing
/// whisper-cli, and a recognised-but-superseded hash is what turns the engine dot amber and
/// raises the update prompt. If the executable-hash fallback stops resolving on Linux, those
/// users silently keep a broken engine.
/// </summary>
public class WhisperCppLinuxExecutableHashTests
{
    // whisper-cli extracted from whisper-vulkan-linux64.zip / whisper-cuda-linux64.zip
    private const string VulkanCurrent = "3e142b4a8c99a4d5cf00cdd4e15fb77c2a842dd0bd83f647675fe1d303e91a03";
    private const string VulkanR2 = "da61c0c1910c103cf8dea855855f072249689ee2310f375e2b79615cbd012c05";
    private const string VulkanBroken191 = "782fac61b9bcfe8f6db22564bb5a2cda2c22550b9ef38064ec5f188bc86dfe79";
    private const string CudaCurrent = "9c93f819e170d0f00a08c256ac2299f819a8e94f3a6f116bac2657c6ab8de62a";
    private const string CudaR2 = "3a4d717745c2d8cf19ca7a954c29c0349a7f158d93a6597bb86e155f0735474a";
    private const string CudaBroken191 = "16a838ae67e248020b9bc65b8584fcc113cf18167c9cfc0a2d98e194fa52cf95";
    private const string Shared184 = "315f46514fc09a4fefe2f7b6ae95cfac5441a03b8be04eed1b5f567d5ccc38de";

    [Theory]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxVulkanExecutable, VulkanCurrent)]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxCudaExecutable, CudaCurrent)]
    public void GetStatus_CurrentArchive_IsUpToDate(string key, string hash)
    {
        Assert.Equal(DownloadHashManager.UpdateStatus.UpToDate, DownloadHashManager.GetStatus(key, hash));
    }

    [Theory]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxVulkanExecutable, VulkanBroken191)]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxCudaExecutable, CudaBroken191)]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxVulkanExecutable, Shared184)]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxCudaExecutable, Shared184)]
    public void GetStatus_BuildWithMissingSharedLibraries_OffersUpdate(string key, string hash)
    {
        Assert.Equal(DownloadHashManager.UpdateStatus.UpdateAvailable, DownloadHashManager.GetStatus(key, hash));
    }

    [Theory]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxVulkanExecutable, VulkanR2)]
    [InlineData(DownloadHashManager.WhisperCpp.LinuxCudaExecutable, CudaR2)]
    public void GetStatus_SupersededButWorkingArchive_OffersUpdate(string key, string hash)
    {
        // The -r2 rebuild is a working install, just an older whisper.cpp - it must still be
        // recognised (not Unknown) so the user gets the normal update prompt.
        Assert.Equal(DownloadHashManager.UpdateStatus.UpdateAvailable, DownloadHashManager.GetStatus(key, hash));
    }

    [Fact]
    public void GetStatus_VulkanAndCudaHashesAreNotInterchangeable()
    {
        // v1.8.5 onwards the two backends produce different whisper-cli binaries, so a CUDA hash
        // must not validate a Vulkan install (v1.8.4 is the documented exception, covered above).
        Assert.Equal(
            DownloadHashManager.UpdateStatus.Unknown,
            DownloadHashManager.GetStatus(DownloadHashManager.WhisperCpp.LinuxVulkanExecutable, CudaCurrent));
        Assert.Equal(
            DownloadHashManager.UpdateStatus.Unknown,
            DownloadHashManager.GetStatus(DownloadHashManager.WhisperCpp.LinuxCudaExecutable, VulkanCurrent));
    }

    [Fact]
    public void ResolveWhisperCppExecutableKey_OnLinux_ResolvesBothBackends()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        Assert.Equal(
            DownloadHashManager.WhisperCpp.LinuxVulkanExecutable,
            DownloadHashManager.ResolveWhisperCppExecutableKey(WhisperChoice.Cpp));
        Assert.Equal(
            DownloadHashManager.WhisperCpp.LinuxCudaExecutable,
            DownloadHashManager.ResolveWhisperCppExecutableKey(WhisperChoice.CppCuBlas));
    }

    [Fact]
    public void ResolveWhisperCppExecutableKey_MatchesArchiveKeyBackend()
    {
        // The executable key and the archive key must name the same backend for a given choice,
        // or the sidecar and no-sidecar paths would disagree about which install is present.
        foreach (var choice in new[] { WhisperChoice.Cpp, WhisperChoice.CppCuBlas, WhisperChoice.CppVulkan })
        {
            var executableKey = DownloadHashManager.ResolveWhisperCppExecutableKey(choice);
            if (executableKey == null)
            {
                continue;
            }

            var archiveKey = DownloadHashManager.ResolveWhisperCppKey(choice);
            Assert.NotNull(archiveKey);
            Assert.Equal(archiveKey + ".Executable", executableKey);
        }
    }
}
