using System.IO;
using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Logic.Download;

/// <summary>
/// llama.cpp ships two CUDA builds (12.4 and 13.3) that unpack into the same folder and share the
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

    [Theory]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsCuda13)]
    [InlineData(DownloadHashManager.LlamaCpp.WindowsCuda13Runtime)]
    public void KnownHashes_ContainsTheCuda13Archives(string key)
    {
        Assert.False(string.IsNullOrEmpty(DownloadHashManager.GetLatestKnownHash(key)));
    }
}
