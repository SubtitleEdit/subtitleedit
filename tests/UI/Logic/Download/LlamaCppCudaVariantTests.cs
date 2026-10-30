using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Logic.Download;

/// <summary>
/// llama.cpp ships two CUDA builds (12.4 and 13.4) that unpack into the same folder and share the
/// same ggml-cuda.dll name, so the only thing telling them apart on disk is the cudart
/// redistributable. These tests pin that detection plus the variant/hash-key round-trip, because
/// getting either wrong silently re-downloads the other CUDA build on an engine update.
/// </summary>
public class LlamaCppCudaVariantTests
{
    private static string MakeInstallFolder(params string[] fileNames)
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-llamacpp-" + Path.GetRandomFileName());
        Directory.CreateDirectory(folder);
        foreach (var name in fileNames)
        {
            File.WriteAllText(Path.Combine(folder, name), string.Empty);
        }

        return folder;
    }

    [Fact]
    public void DetectWindowsVariant_Cuda13Runtime_ReturnsCuda13()
    {
        var folder = MakeInstallFolder("llama-server.exe", "ggml-cuda.dll", "cudart64_13.dll");
        try
        {
            Assert.Equal("cuda13", DownloadHashManager.DetectLlamaCppWindowsVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DetectWindowsVariant_Cuda12Runtime_ReturnsCuda()
    {
        var folder = MakeInstallFolder("llama-server.exe", "ggml-cuda.dll", "cudart64_12.dll");
        try
        {
            Assert.Equal("cuda", DownloadHashManager.DetectLlamaCppWindowsVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Theory]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsCuda, "cuda")]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsCuda13, "cuda13")]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsVulkan, "vulkan")]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsCpu, "cpu")]
    public void GetWindowsVariant_MapsEveryWindowsArchiveKey(string key, string expectedVariant)
    {
        Assert.Equal(expectedVariant, DownloadHashManager.GetLlamaCppWindowsVariant(key));
    }

    [Fact]
    public void DetectLinuxVariant_VulkanBackendLibrary_ReturnsVulkan()
    {
        var folder = MakeInstallFolder("llama-server", "libggml-base.so", "libggml-vulkan.so");
        try
        {
            Assert.Equal("vulkan", DownloadHashManager.DetectLlamaCppLinuxVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DetectLinuxVariant_NoVulkanBackendLibrary_ReturnsCpu()
    {
        var folder = MakeInstallFolder("llama-server", "libggml-base.so");
        try
        {
            Assert.Equal("cpu", DownloadHashManager.DetectLlamaCppLinuxVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DetectLinuxVariant_NoServerBinary_ReturnsNull()
    {
        var folder = MakeInstallFolder("libggml-vulkan.so");
        try
        {
            Assert.Null(DownloadHashManager.DetectLlamaCppLinuxVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Theory]
    [InlineData(LlamaCppDownloadService.VariantCpu, "libggml-vulkan.so", "ggml-vulkan.dll", "ggml-cuda.dll", "cudart64_12.dll", "cudart64_13.dll")]
    [InlineData(LlamaCppDownloadService.VariantVulkan, "ggml-cuda.dll", "cudart64_12.dll", "cudart64_13.dll")]
    [InlineData(LlamaCppDownloadService.VariantCuda, "libggml-vulkan.so", "ggml-vulkan.dll", "cudart64_13.dll")]
    [InlineData(LlamaCppDownloadService.VariantCuda13, "libggml-vulkan.so", "ggml-vulkan.dll", "cudart64_12.dll")]
    public void DeleteOtherBuildFiles_RemovesOnlyWhatTheVariantDoesNotShip(string variant, params string[] expectedDeleted)
    {
        string[] backendFiles = ["libggml-vulkan.so", "ggml-vulkan.dll", "ggml-cuda.dll", "cudart64_12.dll", "cudart64_13.dll"];
        var folder = MakeInstallFolder([.. backendFiles, "llama-server", "libggml-base.so", "custom.txt"]);
        try
        {
            LlamaCppDownloadService.DeleteOtherBuildFiles(folder, variant);

            foreach (var fileName in backendFiles)
            {
                Assert.Equal(!expectedDeleted.Contains(fileName), File.Exists(Path.Combine(folder, fileName)));
            }

            Assert.True(File.Exists(Path.Combine(folder, "llama-server")));
            Assert.True(File.Exists(Path.Combine(folder, "libggml-base.so")));
            Assert.True(File.Exists(Path.Combine(folder, "custom.txt")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Theory]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsCuda13)]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsCuda13Runtime)]
    public void KnownHashes_ContainsTheCuda13Archives(string key)
    {
        Assert.False(string.IsNullOrEmpty(DownloadHashManager.GetLatestKnownHash(key)));
    }
}
