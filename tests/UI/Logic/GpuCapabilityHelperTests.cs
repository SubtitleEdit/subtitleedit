using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The GPU check behind the "Recommended for this computer" line in the CrispASR build prompt
/// (#16944): what is recommended for a given set of adapters, and the text parsing detection
/// relies on. The detection itself reads the registry or /proc, so only its contract is tested.
/// </summary>
public class GpuCapabilityHelperTests
{
    private static readonly GpuInfo Nvidia = new("NVIDIA GeForce RTX 4070", GpuVendor.Nvidia, "581.57", 12L * 1024 * 1024 * 1024);
    private static readonly GpuInfo Amd = new("AMD Radeon RX 7800 XT", GpuVendor.Amd);
    private static readonly GpuInfo Intel = new("Intel(R) UHD Graphics 770", GpuVendor.Intel);

    [Fact]
    public void Recommend_NvidiaWithCudaDriver_IsCuda()
    {
        var capability = new GpuCapability(new[] { Intel, Nvidia }, CudaDriverInstalled: true, VulkanAvailable: true);
        Assert.Equal("cuda", GpuCapabilityHelper.RecommendVariant(capability));
    }

    [Fact]
    public void Recommend_NvidiaWithoutCudaDriver_FallsBackToVulkan()
    {
        var capability = new GpuCapability(new[] { Nvidia }, CudaDriverInstalled: false, VulkanAvailable: true);
        Assert.Equal("vulkan", GpuCapabilityHelper.RecommendVariant(capability));
    }

    [Fact]
    public void Recommend_CudaDriverLeftBehindWithoutNvidiaAdapter_IsNotCuda()
    {
        var capability = new GpuCapability(new[] { Amd }, CudaDriverInstalled: true, VulkanAvailable: true);
        Assert.Equal("vulkan", GpuCapabilityHelper.RecommendVariant(capability));
    }

    [Theory]
    [InlineData(true, "vulkan")]
    [InlineData(false, "cpu")]
    public void Recommend_AmdOrIntel_IsVulkanOnlyWithALoader(bool vulkanAvailable, string expected)
    {
        Assert.Equal(expected, GpuCapabilityHelper.RecommendVariant(new GpuCapability(new[] { Amd }, false, vulkanAvailable)));
        Assert.Equal(expected, GpuCapabilityHelper.RecommendVariant(new GpuCapability(new[] { Intel }, false, vulkanAvailable)));
    }

    [Fact]
    public void Recommend_NoAdapter_IsCpu()
    {
        Assert.Equal("cpu", GpuCapabilityHelper.RecommendVariant(GpuCapability.None));
        Assert.Equal("cpu", GpuCapabilityHelper.RecommendVariant(new GpuCapability(Array.Empty<GpuInfo>(), false, true)));
    }

    [Fact]
    public void PrimaryGpu_PrefersNvidiaOverIntegrated()
    {
        var capability = new GpuCapability(new[] { Intel, Amd, Nvidia }, true, true);
        Assert.Same(Nvidia, GpuCapabilityHelper.GetPrimaryGpu(capability));
        Assert.Null(GpuCapabilityHelper.GetPrimaryGpu(GpuCapability.None));
    }

    [Fact]
    public void Describe_ListsOnlyWhatIsKnown()
    {
        Assert.Equal("NVIDIA GeForce RTX 4070 (driver 581.57, 12 GB)", GpuCapabilityHelper.Describe(Nvidia));
        Assert.Equal("AMD Radeon RX 7800 XT", GpuCapabilityHelper.Describe(Amd));
        Assert.Equal("Old card (512 MB)", GpuCapabilityHelper.Describe(new GpuInfo("Old card", GpuVendor.Unknown, null, 512L * 1024 * 1024)));
    }

    [Theory]
    [InlineData(33715493478L, "32 GB")] // 31.4 GiB, what the driver reports for a 32 GB card
    [InlineData(17093516493L, "16 GB")] // 15.9 GiB
    [InlineData(12884901888L, "12 GB")] // exactly 12 GiB
    [InlineData(11811160064L, "11 GB")] // exactly 11 GiB
    [InlineData(8160437862L, "8 GB")]   // 7.6 GiB
    public void Describe_RoundsMemoryUpToTheNominalSize(long bytes, string expected)
    {
        Assert.Equal($"Card ({expected})", GpuCapabilityHelper.Describe(new GpuInfo("Card", GpuVendor.Unknown, null, bytes)));
    }

    [Theory]
    [InlineData("32.0.15.8157", "581.57")]
    [InlineData("31.0.15.3623", "536.23")]
    [InlineData("27.21.14.5671", "456.71")]
    [InlineData("9.18.13.4144", "341.44")]
    [InlineData("32.0.15.812", "508.12")]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData("580.95.05", null)]
    [InlineData("a.b.c.d", null)]
    public void NvidiaVersion_ComesFromTheLastFiveDigits(string? windowsVersion, string? expected)
    {
        Assert.Equal(expected, GpuCapabilityHelper.NvidiaVersionFromWindowsDriverVersion(windowsVersion));
    }

    [Theory]
    [InlineData("NVRM version: NVIDIA UNIX x86_64 Kernel Module  580.95.05  Tue Sep 23 10:11:27 UTC 2025\nGCC version:  gcc version 13.3.0", "580.95.05")]
    [InlineData("NVRM version: NVIDIA UNIX Open Kernel Module for x86_64  570.133.07  Release Build  (dvs-builder@U16-I1-N07-12-3)", "570.133.07")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void LinuxNvidiaDriverVersion_IsReadFromProc(string? text, string? expected)
    {
        Assert.Equal(expected, GpuCapabilityHelper.ParseLinuxNvidiaDriverVersion(text));
    }

    [Fact]
    public void LinuxNvidiaModel_IsReadFromInformation()
    {
        const string information = "Model: \t\t NVIDIA GeForce RTX 5090\nIRQ:   \t\t 187\nGPU UUID: \t GPU-00000000\nBus Type: \t PCIe\n";
        Assert.Equal("NVIDIA GeForce RTX 5090", GpuCapabilityHelper.ParseLinuxNvidiaModel(information));
        Assert.Null(GpuCapabilityHelper.ParseLinuxNvidiaModel("IRQ: 187"));
    }

    [Theory]
    [InlineData(null, "10DE", GpuVendor.Nvidia)]
    [InlineData(null, "0x10de\n", GpuVendor.Nvidia)]
    [InlineData(null, "0x1002", GpuVendor.Amd)]
    [InlineData(null, "0x8086", GpuVendor.Intel)]
    [InlineData("NVIDIA GeForce RTX 4070", null, GpuVendor.Nvidia)]
    [InlineData("AMD Radeon(TM) Graphics", null, GpuVendor.Amd)]
    [InlineData("Intel(R) Arc(TM) A770 Graphics", null, GpuVendor.Intel)]
    [InlineData("Microsoft Basic Display Adapter", null, GpuVendor.Unknown)]
    [InlineData("Microsoft Remote Display Adapter", "1414", GpuVendor.Unknown)]
    public void Vendor_ComesFromThePciIdOrTheName(string? name, string? vendorId, GpuVendor expected)
    {
        Assert.Equal(expected, GpuCapabilityHelper.GetVendor(name, vendorId));
    }

    [Fact]
    public void Detect_NeverThrowsAndIsConsistent()
    {
        var capability = GpuCapabilityHelper.Detect();
        Assert.NotNull(capability);
        Assert.Same(capability, GpuCapabilityHelper.Detect());
        Assert.Contains(GpuCapabilityHelper.RecommendVariant(capability), new[] { "cpu", "vulkan", "cuda" });
    }

    [Fact]
    public void Hint_NamesTheAdapterAndTheBuild()
    {
        var text = GpuBuildHint.Create(new GpuCapability(new[] { Nvidia }, true, true), new[] { GpuBuild.Cpu, GpuBuild.Vulkan, GpuBuild.Cuda }).Hint;
        Assert.Contains("NVIDIA GeForce RTX 4070 (driver 581.57, 12 GB)", text);
        Assert.EndsWith("CUDA", text);

        var none = GpuBuildHint.Create(GpuCapability.None, new[] { GpuBuild.Cpu, GpuBuild.Vulkan, GpuBuild.Cuda }).Hint;
        Assert.EndsWith("CPU", none);
    }

    [Fact]
    public void Hint_ListsEveryAdapter_NvidiaFirstThenByMemory()
    {
        var second = new GpuInfo("NVIDIA GeForce RTX 3080 Ti", GpuVendor.Nvidia, "581.57", 12L * 1024 * 1024 * 1024);
        var first = new GpuInfo("NVIDIA GeForce RTX 5080", GpuVendor.Nvidia, "581.57", 16L * 1024 * 1024 * 1024);
        var text = GpuBuildHint.Create(new GpuCapability(new[] { Intel, second, first }, true, true), new[] { GpuBuild.Cpu, GpuBuild.Vulkan, GpuBuild.Cuda }).Hint;

        var lines = text.Split(Environment.NewLine);
        Assert.Equal(5, lines.Length);
        Assert.Equal("Detected:", lines[0]);
        Assert.Equal("- NVIDIA GeForce RTX 5080 (driver 581.57, 16 GB)", lines[1]);
        Assert.Equal("- NVIDIA GeForce RTX 3080 Ti (driver 581.57, 12 GB)", lines[2]);
        Assert.Equal("- Intel(R) UHD Graphics 770", lines[3]);
        Assert.EndsWith("CUDA", lines[4]);
    }

    [Fact]
    public void DefaultButton_IsTheRecommendedBuildsButton()
    {
        GpuBuild[] cpuVulkanCuda = { GpuBuild.Cpu, GpuBuild.Vulkan, GpuBuild.Cuda };
        Assert.Equal(MessageBoxResult.Custom3, GpuBuildHint.Create(new GpuCapability(new[] { Nvidia }, true, true), cpuVulkanCuda).DefaultButton);
        Assert.Equal(MessageBoxResult.Custom2, GpuBuildHint.Create(new GpuCapability(new[] { Amd }, false, true), cpuVulkanCuda).DefaultButton);
        Assert.Equal(MessageBoxResult.Custom1, GpuBuildHint.Create(GpuCapability.None, cpuVulkanCuda).DefaultButton);

        // Linux llama.cpp: no CUDA button, so an NVIDIA card gets Vulkan
        Assert.Equal(MessageBoxResult.Custom2, GpuBuildHint.Create(new GpuCapability(new[] { Nvidia }, true, true), new[] { GpuBuild.Cpu, GpuBuild.Vulkan }).DefaultButton);

        // Linux CrispASR: ROCm is never recommended (not detected), AMD gets Vulkan
        Assert.Equal(MessageBoxResult.Custom2, GpuBuildHint.Create(new GpuCapability(new[] { Amd }, false, true), new[] { GpuBuild.Cpu, GpuBuild.Vulkan, GpuBuild.Cuda, GpuBuild.Rocm }).DefaultButton);
    }

    [Theory]
    [InlineData("581.57", 0, GpuBuild.Cuda13)]   // driver 580+ supports CUDA 13
    [InlineData("560.94", 0, GpuBuild.Cuda12)]   // too old for CUDA 13
    [InlineData("581.57", 12080, GpuBuild.Cuda12)] // the driver's own answer wins over the version number
    [InlineData("581.57", 13000, GpuBuild.Cuda13)]
    [InlineData(null, 0, GpuBuild.Cuda12)]       // unknown driver: the safe choice
    public void CudaVersionPrompt_RecommendsCuda13OnlyWhenTheDriverSupportsIt(string? driver, int cudaVersion, GpuBuild expected)
    {
        var gpu = new GpuInfo("NVIDIA GeForce RTX 4070", GpuVendor.Nvidia, driver, null, 0, new Version(8, 9));
        var capability = new GpuCapability(new[] { gpu }, true, true, cudaVersion);
        Assert.Equal(expected, GpuBuildHint.Recommend(capability, GpuBuild.Cuda12, GpuBuild.Cuda13));
    }

    [Fact]
    public void CudaVersionPrompt_PascalCardGetsCuda12AndTheReason()
    {
        var gtx1080 = new GpuInfo("NVIDIA GeForce GTX 1080", GpuVendor.Nvidia, "581.57", null, 0, new Version(6, 1));
        var prompt = GpuBuildHint.Create(new GpuCapability(new[] { gtx1080 }, true, true, 13000), new[] { GpuBuild.Cuda12, GpuBuild.Cuda13 });

        Assert.Equal(GpuBuild.Cuda12, prompt.Recommended);
        Assert.Equal(MessageBoxResult.Custom1, prompt.DefaultButton);
        Assert.EndsWith("CUDA 13 does not support this card (compute capability 6.1).", prompt.Hint);
    }

    [Fact]
    public void CudaVersionPrompt_OldDriverGivesTheReason()
    {
        var gpu = new GpuInfo("NVIDIA GeForce RTX 3060", GpuVendor.Nvidia, "560.94");
        var prompt = GpuBuildHint.Create(new GpuCapability(new[] { gpu }, true, true), new[] { GpuBuild.Cuda12, GpuBuild.Cuda13 });

        Assert.Equal(GpuBuild.Cuda12, prompt.Recommended);
        Assert.EndsWith("CUDA 13 needs NVIDIA driver 580 or newer.", prompt.Hint);
    }

    [Fact]
    public void KeplerCard_IsNotRecommendedCuda()
    {
        var gtx780 = new GpuInfo("NVIDIA GeForce GTX 780", GpuVendor.Nvidia, "474.44", null, 0, new Version(3, 5));
        var prompt = GpuBuildHint.Create(new GpuCapability(new[] { gtx780 }, true, true), new[] { GpuBuild.Cpu, GpuBuild.Vulkan, GpuBuild.Cuda });

        Assert.Equal(GpuBuild.Vulkan, prompt.Recommended);
        Assert.EndsWith("The NVIDIA card or driver is too old for the CUDA build.", prompt.Hint);
    }

    [Theory]
    [InlineData("581.57", GpuBuild.Cuda12)]
    [InlineData("472.12", GpuBuild.Cuda11)] // Paddle OCR's CUDA 11 build for a driver older than CUDA 12 needs
    public void PaddleOcr_PicksTheCudaVersionTheDriverRuns(string driver, GpuBuild expected)
    {
        var gpu = new GpuInfo("NVIDIA GeForce RTX 2060", GpuVendor.Nvidia, driver);
        Assert.Equal(expected, GpuBuildHint.Recommend(new GpuCapability(new[] { gpu }, true, true), GpuBuild.Cpu, GpuBuild.Cuda11, GpuBuild.Cuda12));
    }

    [Fact]
    public void CudaVersion_IsTheDevice0ComputeThatCounts()
    {
        // A fast card as device 0 and an old one as device 1: engines run on device 0.
        var rtx = new GpuInfo("NVIDIA GeForce RTX 5080", GpuVendor.Nvidia, "581.57", null, 0, new Version(12, 0));
        var gtx = new GpuInfo("NVIDIA GeForce GTX 1080", GpuVendor.Nvidia, "581.57", null, 1, new Version(6, 1));
        Assert.Equal(GpuBuild.Cuda13, GpuBuildHint.Recommend(new GpuCapability(new[] { gtx, rtx }, true, true, 13000), GpuBuild.Cuda12, GpuBuild.Cuda13));
    }

    [Fact]
    public void MessageBox_PreferredDefaultWinsOnlyWhenItIsAButton()
    {
        var preferred = MessageBox.GetButtonLayout(MessageBoxButtons.Cancel, "CPU", "Vulkan", "CUDA", preferredDefault: MessageBoxResult.Custom3);
        Assert.Equal(MessageBoxResult.Custom3, preferred.Single(b => b.IsDefault).Result);
        Assert.Equal(
            new[] { MessageBoxResult.Cancel, MessageBoxResult.Custom1, MessageBoxResult.Custom2, MessageBoxResult.Custom3 },
            preferred.Select(b => b.Result));

        // Custom4 is not shown here, so the usual default (the first custom button) stays.
        var missing = MessageBox.GetButtonLayout(MessageBoxButtons.Cancel, "CPU", "Vulkan", "CUDA", preferredDefault: MessageBoxResult.Custom4);
        Assert.Equal(MessageBoxResult.Custom1, missing.Single(b => b.IsDefault).Result);

        var unset = MessageBox.GetButtonLayout(MessageBoxButtons.Cancel, "CPU", "Vulkan", "CUDA");
        Assert.Equal(MessageBoxResult.Custom1, unset.Single(b => b.IsDefault).Result);
    }

    [Fact]
    public void WhisperCppBackend_RecommendationOnlyAppliesWhileNothingIsInstalled()
    {
        var engine = new WhisperCppEngine();
        var anyInstalled = engine.Backends.Any(b => b.IsEngineInstalled());
        var before = engine.SelectedBackend;

        var changed = engine.SelectRecommendedBackendIfNoneInstalled(new GpuCapability(new[] { Nvidia }, true, true));

        if (engine.Backends.Count < 2 || anyInstalled)
        {
            // One backend (macOS, Linux), or the user already downloaded one: never touched.
            Assert.False(changed);
            Assert.Same(before, engine.SelectedBackend);
        }
        else
        {
            Assert.True(changed);
            Assert.IsType<WhisperEngineCppCuBlas>(engine.SelectedBackend);
        }
    }

    [Fact]
    public void CudaNumbers_AreMatchedByName_AndOrderTheList()
    {
        // The registry lists the cards in slot order; CUDA numbers the faster one first.
        var slot0 = new GpuInfo("NVIDIA GeForce RTX 3080 Ti", GpuVendor.Nvidia, "581.57", 12L * 1024 * 1024 * 1024);
        var slot1 = new GpuInfo("NVIDIA GeForce RTX 5080", GpuVendor.Nvidia, "581.57", 16L * 1024 * 1024 * 1024);
        var capability = new GpuCapability(new[] { Intel, slot0, slot1 }, true, true);
        var cuda = new[]
        {
            new CudaDevice(0, "NVIDIA GeForce RTX 5080", 16L * 1024 * 1024 * 1024),
            new CudaDevice(1, "NVIDIA GeForce RTX 3080 Ti", 12L * 1024 * 1024 * 1024),
        };

        var numbered = GpuCapabilityHelper.WithCudaDeviceNumbers(capability, cuda);
        var shown = GpuCapabilityHelper.GetGpusForDisplay(numbered);

        Assert.Equal(3, shown.Count);
        Assert.Equal("NVIDIA GeForce RTX 5080 (CUDA device 0, driver 581.57, 16 GB)", GpuCapabilityHelper.Describe(shown[0]));
        Assert.Equal("NVIDIA GeForce RTX 3080 Ti (CUDA device 1, driver 581.57, 12 GB)", GpuCapabilityHelper.Describe(shown[1]));
        Assert.Equal("Intel(R) UHD Graphics 770", GpuCapabilityHelper.Describe(shown[2]));
    }

    [Fact]
    public void CudaNumbers_TwoCardsOfOneModel_EachGetsANumber()
    {
        var capability = new GpuCapability(new[] { Nvidia, Nvidia }, true, true);
        var cuda = new[] { new CudaDevice(0, Nvidia.Name, 0), new CudaDevice(1, Nvidia.Name, 0) };

        var shown = GpuCapabilityHelper.GetGpusForDisplay(GpuCapabilityHelper.WithCudaDeviceNumbers(capability, cuda));

        Assert.Equal(new int?[] { 0, 1 }, shown.Select(g => g.CudaIndex));
    }

    [Fact]
    public void CudaNumbers_FillInWhatLinuxProcDoesNotGive()
    {
        // /proc gives no memory size, and no model for a card it only knows by vendor.
        var capability = new GpuCapability(new[] { new GpuInfo("NVIDIA GPU", GpuVendor.Nvidia, "580.95.05") }, true, true);
        var cuda = new[] { new CudaDevice(0, "NVIDIA GeForce RTX 5090", 32L * 1024 * 1024 * 1024) };

        var gpu = Assert.Single(GpuCapabilityHelper.WithCudaDeviceNumbers(capability, cuda).Gpus);

        Assert.Equal("NVIDIA GeForce RTX 5090 (CUDA device 0, driver 580.95.05, 32 GB)", GpuCapabilityHelper.Describe(gpu));
    }

    [Fact]
    public void CudaNumbers_NoDevicesOrNoDriver_ChangeNothing()
    {
        var capability = new GpuCapability(new[] { Nvidia }, true, true);
        Assert.Same(capability, GpuCapabilityHelper.WithCudaDeviceNumbers(capability, Array.Empty<CudaDevice>()));

        var noDriver = new GpuCapability(new[] { Amd }, false, true);
        Assert.Same(noDriver, GpuCapabilityHelper.WithCudaDeviceNumbers(noDriver));

        Assert.NotNull(CudaDeviceEnumerator.GetDevices());
    }
}
