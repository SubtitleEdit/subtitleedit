using System.IO;
using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Logic.Download;

/// <summary>
/// CrispASR v0.8.31 added a Windows CUDA 13 build beside the CUDA 12 one (#14343). Both unpack
/// into the same folder and ship the same ggml-cuda.dll name, so detection has to go by the
/// install sidecar (or, for an install made before CUDA 13 existed, the bundled cudart
/// redistributable). Getting it wrong is not cosmetic: TtsVoiceInstaller re-downloads "the variant
/// the user originally picked", so a CUDA 13 install reported as "cuda" is silently replaced by
/// the CUDA 12 archive.
/// </summary>
public class CrispAsrCudaVariantTests
{
    private static string MakeInstallFolder(params string[] fileNames)
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-crispasr-" + Path.GetRandomFileName());
        Directory.CreateDirectory(folder);
        foreach (var name in fileNames)
        {
            File.WriteAllText(Path.Combine(folder, name), string.Empty);
        }

        return folder;
    }

    private static void WriteSidecar(string folder, string key)
    {
        File.WriteAllLines(Path.Combine(folder, ".installed.sha256"), new[] { key, new string('0', 64) });
    }

    [Fact]
    public void DetectWindowsVariant_Cuda13Sidecar_ReturnsCuda13()
    {
        var folder = MakeInstallFolder("crispasr.exe", "ggml-cuda.dll");
        WriteSidecar(folder, DownloadHashManager.CrispAsr.WindowsCuda13);
        try
        {
            Assert.Equal("cuda13", DownloadHashManager.DetectCrispAsrWindowsVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DetectWindowsVariant_Cuda12Sidecar_ReturnsCuda()
    {
        var folder = MakeInstallFolder("crispasr.exe", "ggml-cuda.dll");
        WriteSidecar(folder, DownloadHashManager.CrispAsr.WindowsCuda);
        try
        {
            Assert.Equal("cuda", DownloadHashManager.DetectCrispAsrWindowsVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DetectWindowsVariant_NoSidecarButCuda13Runtime_ReturnsCuda13()
    {
        var folder = MakeInstallFolder("crispasr.exe", "ggml-cuda.dll", "cudart64_13.dll");
        try
        {
            Assert.Equal("cuda13", DownloadHashManager.DetectCrispAsrWindowsVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DetectWindowsVariant_NoSidecarAndCuda12Runtime_ReturnsCuda()
    {
        var folder = MakeInstallFolder("crispasr.exe", "ggml-cuda.dll", "cudart64_12.dll");
        try
        {
            Assert.Equal("cuda", DownloadHashManager.DetectCrispAsrWindowsVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void DetectWindowsVariant_VulkanIsUnaffected()
    {
        var folder = MakeInstallFolder("crispasr.exe", "ggml-vulkan.dll");
        try
        {
            Assert.Equal("vulkan", DownloadHashManager.DetectCrispAsrWindowsVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Theory]
    [InlineData("cuda", DownloadHashManager.CrispAsr.WindowsCuda, DownloadHashManager.CrispAsr.WindowsCudaExecutable)]
    [InlineData("cuda13", DownloadHashManager.CrispAsr.WindowsCuda13, DownloadHashManager.CrispAsr.WindowsCuda13Executable)]
    public void EveryDetectedWindowsCudaVariantResolvesBackToItsOwnHashKeys(
        string variant, string archiveKey, string executableKey)
    {
        // The detected variant feeds both the re-download and the "is this install up to date?"
        // check, so it has to survive the round trip through the hash-key lookups.
        Assert.Equal(variant, DownloadHashManager.GetCrispAsrVariant(archiveKey));
        Assert.Equal(variant, DownloadHashManager.GetCrispAsrVariant(executableKey));
        Assert.Equal(variant, DownloadHashManager.GetCrispAsrWindowsVariant(archiveKey));
        Assert.False(string.IsNullOrEmpty(DownloadHashManager.GetLatestKnownHash(archiveKey)));
        Assert.False(string.IsNullOrEmpty(DownloadHashManager.GetLatestKnownHash(executableKey)));
    }
}
