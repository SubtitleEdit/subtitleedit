using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.EngineSettings;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace UITests.Features.Translate;

/// <summary>
/// The CrispASR build choice for MADLAD: picking a build on install, switching it through the
/// engine gear, and keeping it on update, since speech-to-text runs the same binary (issue #16532).
/// </summary>
public class CrispAsrGpuTests
{
    [AvaloniaTheory]
    [InlineData(MessageBoxResult.Custom1, "")]
    [InlineData(MessageBoxResult.Custom2, "vulkan")]
    [InlineData(MessageBoxResult.Custom4, "hip")]
    [InlineData(MessageBoxResult.Cancel, null)]
    [InlineData(MessageBoxResult.None, null)]
    public async Task LinuxVariant_SelectsBuildOrCancels(MessageBoxResult answer, string? expected)
    {
        var owner = new Window();
        owner.Show();
        try
        {
            var result = CrispAsrDownloadHelper.PromptLinuxVariantAsync(owner, "MADLAD");
            ClosePrompt(owner, answer);
            Assert.Equal(expected, await result);
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(false, MessageBoxResult.Custom1, "cuda")]
    [InlineData(false, MessageBoxResult.Custom2, "cuda13")]
    [InlineData(false, MessageBoxResult.Cancel, null)]
    [InlineData(true, MessageBoxResult.Custom1, "cuda")]
    [InlineData(true, MessageBoxResult.Custom2, "cuda13")]
    [InlineData(true, MessageBoxResult.Cancel, null)]
    public async Task CudaVariant_SelectsVersionOrCancels(bool windows, MessageBoxResult answer, string? expected)
    {
        var owner = new Window();
        owner.Show();
        try
        {
            var result = windows
                ? CrispAsrDownloadHelper.PromptWindowsVariantAsync(owner, "MADLAD")
                : CrispAsrDownloadHelper.PromptLinuxVariantAsync(owner, "MADLAD");
            ClosePrompt(owner, MessageBoxResult.Custom3);
            await Task.Yield();
            ClosePrompt(owner, answer);
            Assert.Equal(expected, await result);
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(MessageBoxResult.Custom1, "cpu")]
    [InlineData(MessageBoxResult.Custom2, "cpu-legacy")]
    [InlineData(MessageBoxResult.Cancel, null)]
    public async Task WindowsCpuVariant_PreservesStandardAndLegacy(MessageBoxResult answer, string? expected)
    {
        var owner = new Window();
        owner.Show();
        try
        {
            var result = CrispAsrDownloadHelper.PromptWindowsVariantAsync(owner, "MADLAD");
            ClosePrompt(owner, MessageBoxResult.Custom1);
            await Task.Yield();
            ClosePrompt(owner, answer);
            Assert.Equal(expected, await result);
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaFact]
    public async Task WindowsVariant_SelectsVulkanAfterRuntimeWarning()
    {
        var owner = new Window();
        owner.Show();
        try
        {
            var result = CrispAsrDownloadHelper.PromptWindowsVariantAsync(owner, "MADLAD");
            ClosePrompt(owner, MessageBoxResult.Custom2);
            if (!VulkanHelper.IsInstalled())
            {
                await Task.Yield();
                ClosePrompt(owner, MessageBoxResult.Yes);
            }

            Assert.Equal("vulkan", await result);
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaFact]
    public async Task WindowsVariant_CancelDoesNotSelectDefaultGpuBuild()
    {
        var owner = new Window();
        owner.Show();
        try
        {
            var result = CrispAsrDownloadHelper.PromptWindowsVariantAsync(owner, "MADLAD");
            ClosePrompt(owner, MessageBoxResult.Cancel);
            Assert.Null(await result);
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaFact]
    public async Task SwitchBuild_CancelDoesNotStartDownload()
    {
        if (!CrispAsrDownloadHelper.HasVariantChoice)
        {
            return;
        }

        var owner = new Window();
        owner.Show();
        var services = new ServiceCollection();
        using var provider = services.BuildServiceProvider();
        try
        {
            // No download services are registered: reaching the download dialog would fail.
            var result = CrispAsrTranslateDownloadHelper.UpdateEngineAsync(owner, new WindowService(provider), askForBuild: true);
            ClosePrompt(owner, MessageBoxResult.Cancel);
            Assert.False(await result);
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(MessageBoxResult.Custom1, "cpu")]
    [InlineData(MessageBoxResult.Custom2, "vulkan")]
    [InlineData(MessageBoxResult.Custom3, "cuda")]
    [InlineData(MessageBoxResult.Custom3, "cuda13")]
    [InlineData(MessageBoxResult.Custom4, "hip")]
    public async Task SwitchBuild_RoutesChosenBuildToDownloader(MessageBoxResult answer, string expected)
    {
        if (!CrispAsrDownloadHelper.HasVariantChoice || (OperatingSystem.IsWindows() && expected == "hip"))
        {
            return;
        }

        var owner = new Window();
        owner.Show();
        var download = new PendingDownloadService();
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        services.AddSingleton<ICrispAsrDownloadService>(download);
        using var provider = services.BuildServiceProvider();
        try
        {
            var result = CrispAsrTranslateDownloadHelper.UpdateEngineAsync(owner, new WindowService(provider), askForBuild: true);
            ClosePrompt(owner, answer);
            if (answer == MessageBoxResult.Custom3)
            {
                await Task.Yield();
                ClosePrompt(owner, expected == "cuda13" ? MessageBoxResult.Custom2 : MessageBoxResult.Custom1);
            }
            else if (OperatingSystem.IsWindows() && answer == MessageBoxResult.Custom1)
            {
                await Task.Yield();
                ClosePrompt(owner, MessageBoxResult.Custom1);
            }
            else if (OperatingSystem.IsWindows() && answer == MessageBoxResult.Custom2 && !VulkanHelper.IsInstalled())
            {
                await Task.Yield();
                ClosePrompt(owner, MessageBoxResult.Yes);
            }

            await Task.Yield();
            Dispatcher.UIThread.RunJobs();
            var dialog = owner.OwnedWindows.OfType<DownloadSpeechToTextEngineWindow>().Single();
            var vm = Assert.IsType<DownloadSpeechToTextEngineViewModel>(dialog.DataContext);
            Assert.IsType<CrispAsrMadlad>(vm.Engine);
            Assert.Equal(expected, download.RequestedVariant);
            vm.CommandCancelCommand.Execute(null);
            Assert.False(await result);
        }
        finally
        {
            foreach (var dialog in owner.OwnedWindows.ToList())
            {
                dialog.Close();
            }

            owner.Close();
        }
    }

    [Theory]
    [InlineData(DownloadHashManager.CrispAsr.LinuxVulkan, "vulkan")]
    [InlineData(DownloadHashManager.CrispAsr.LinuxHip, "hip")]
    [InlineData(DownloadHashManager.CrispAsr.Linux, "")]
    [InlineData(DownloadHashManager.CrispAsr.WindowsCuda13, "cuda13")]
    [InlineData(DownloadHashManager.CrispAsr.WindowsCpu, "cpu")]
    public void GetInstalledVariant_ReadsTheSidecar(string key, string expected)
    {
        var folder = MakeInstallFolder(key);
        try
        {
            Assert.Equal(expected, CrispAsrDownloadHelper.GetInstalledVariant(folder));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [AvaloniaFact]
    public async Task Update_KeepsInstalledBuildWithoutAsking()
    {
        var folder = MakeInstallFolder(DownloadHashManager.CrispAsr.LinuxVulkan);
        var owner = new Window();
        owner.Show();
        try
        {
            // A prompt would never be answered here, so the task only completes without one.
            var variant = await CrispAsrTranslateDownloadHelper.ResolveBuildAsync(owner, "MADLAD", folder, isInstalled: true, askForBuild: false);

            Assert.Equal("vulkan", variant);
            Assert.Empty(owner.OwnedWindows);
        }
        finally
        {
            owner.Close();
            Directory.Delete(folder, true);
        }
    }

    [AvaloniaFact]
    public async Task SwitchBuild_WarnsThatSpeechToTextSharesTheEngine()
    {
        if (!CrispAsrDownloadHelper.HasVariantChoice)
        {
            return;
        }

        var folder = MakeInstallFolder(DownloadHashManager.CrispAsr.LinuxVulkan);
        var owner = new Window();
        owner.Show();
        try
        {
            var result = CrispAsrTranslateDownloadHelper.ResolveBuildAsync(owner, "MADLAD", folder, isInstalled: true, askForBuild: true);
            Dispatcher.UIThread.RunJobs();
            var expected = string.Format(Se.Language.Translate.CrispAsrSwitchBuildInfo, "MADLAD");
            var prompt = owner.OwnedWindows.OfType<MessageBox>().Single();
            Assert.Contains(prompt.GetLogicalDescendants().OfType<TextBlock>(), t => t.Text?.Contains(expected) == true);
            ClosePrompt(owner, MessageBoxResult.Cancel);
            Assert.Null(await result);
        }
        finally
        {
            owner.Close();
            Directory.Delete(folder, true);
        }
    }

    [AvaloniaFact]
    public void EngineSettings_DescribesMadladAsATranslationEngine()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<SpeechToTextEngineSettingsViewModel>();
        var window = new SpeechToTextEngineSettingsWindow(vm);
        Assert.Equal(Se.Language.Video.AudioToText.EngineSettings, window.Title);

        vm.Initialize(new CrispAsrMadlad(), () => Task.CompletedTask);

        Assert.Equal(Se.Language.Translate.EngineSettings, window.Title);
        Assert.Equal(Se.Language.Translate.EngineSettingsSubtitle, vm.SubtitleText);
    }

    [AvaloniaFact]
    public void AutoTranslate_EngineGearFollowsMadladAndIsOffDuringARun()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AutoTranslateViewModel>();
        var lastName = Se.Settings.AutoTranslate.AutoTranslateLastName;
        var window = new AutoTranslateWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var gear = window.GetLogicalDescendants().OfType<Button>()
                .Single(b => b.Command == vm.ShowCrispAsrEngineSettingsCommand);

            // The bindings themselves: picking an engine here would depend on the settings the
            // window restored, which other tests change.
            vm.CrispAsrModelComboIsVisible = true;
            vm.IsTranslateEnabled = true;
            Assert.True(gear.IsVisible);
            Assert.True(gear.IsEnabled);

            vm.IsTranslateEnabled = false;
            Assert.False(gear.IsEnabled);

            vm.CrispAsrModelComboIsVisible = false;
            Assert.False(gear.IsVisible);
            vm.IsTranslateEnabled = true;
        }
        finally
        {
            window.Close();
            Se.Settings.AutoTranslate.AutoTranslateLastName = lastName;
        }
    }

    [AvaloniaFact]
    public void BatchConvert_EngineGearIsVisibleOnlyForMadlad()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<BatchConvertViewModel>();
        var view = ViewAutoTranslate.Make(vm);
        view.DataContext = vm;
        var gear = view.GetLogicalDescendants().OfType<Button>()
            .Single(b => b.Command == vm.ShowCrispAsrEngineSettingsCommand);

        vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is CrispAsrMadladTranslate);
        vm.OnAutoTranslatorChanged();
        Assert.True(gear.IsVisible);
        Assert.True(gear.IsEnabled);

        vm.IsConverting = true;
        Assert.False(gear.IsEnabled);
        vm.IsConverting = false;

        vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is not CrispAsrMadladTranslate);
        vm.OnAutoTranslatorChanged();
        Assert.False(gear.IsVisible);
    }

    private static string MakeInstallFolder(string installedKey)
    {
        var folder = Path.Combine(Path.GetTempPath(), "CrispAsrGpuTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, ".installed.sha256"), installedKey + "\nabc\n");
        return folder;
    }

    private static void ClosePrompt(Window owner, MessageBoxResult answer)
    {
        Dispatcher.UIThread.RunJobs();
        owner.OwnedWindows.OfType<MessageBox>().Single().Close(answer);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class PendingDownloadService : ICrispAsrDownloadService
    {
        public string? RequestedVariant { get; private set; }

        private Task Download(string variant, CancellationToken cancellationToken)
        {
            RequestedVariant = variant;
            return Task.Delay(Timeout.Infinite, cancellationToken);
        }

        public Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("cpu", token);
        public Task DownloadEngineWindowsCuda(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("cuda", token);
        public Task DownloadEngineWindowsCuda13(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("cuda13", token);
        public Task DownloadEngineWindowsVulkan(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("vulkan", token);
        public Task DownloadEngineWindowsCpu(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("cpu", token);
        public Task DownloadEngineWindowsCpuLegacy(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("cpu-legacy", token);
        public Task DownloadEngineLinuxCuda(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("cuda", token);
        public Task DownloadEngineLinuxCuda13(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("cuda13", token);
        public Task DownloadEngineLinuxVulkan(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("vulkan", token);
        public Task DownloadEngineLinuxHip(Stream stream, IProgress<float>? progress, CancellationToken token) => Download("hip", token);
    }
}
