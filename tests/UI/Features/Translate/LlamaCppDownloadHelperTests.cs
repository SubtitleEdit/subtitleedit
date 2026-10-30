using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace UITests.Features.Translate;

/// <summary>
/// The Linux build picker has to appear both on a first install and on a re-download of an installed
/// engine: otherwise Linux can only ever install the CPU build and -ngl has no effect (#16488).
/// </summary>
public class LlamaCppDownloadHelperTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Linux_EngineDownloadOffersCpuAndVulkan_EvenWhenAlreadyInstalled(bool installed)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var folder = Path.Combine(Path.GetTempPath(), "se-llamacpp-" + Path.GetRandomFileName());
        var originalFolder = LlamaCppServerManager.FolderOverride;
        var originalExecutable = LlamaCppServerManager.ExecutableOverride;
        Directory.CreateDirectory(folder);
        LlamaCppServerManager.FolderOverride = folder;
        LlamaCppServerManager.ExecutableOverride = null;
        var owner = new Window();
        try
        {
            if (installed)
            {
                File.WriteAllText(LlamaCppServerManager.GetExecutable(), string.Empty);
            }

            owner.Show();
            var download = LlamaCppDownloadHelper.DownloadAsync(
                owner, new StubWindowService(), null, forceEngineDownload: installed);
            Dispatcher.UIThread.RunJobs();

            var picker = Assert.Single(owner.OwnedWindows.OfType<MessageBox>());
            var buttons = picker.GetLogicalDescendants().OfType<Button>().Select(b => b.Content).ToList();
            Assert.Contains("CPU", buttons);
            Assert.Contains("Vulkan (GPU)", buttons);
            Assert.DoesNotContain("CUDA (NVIDIA GPU)", buttons);

            picker.Close(MessageBoxResult.Cancel);
            Assert.Null(await download);
        }
        finally
        {
            foreach (var dialog in owner.OwnedWindows.ToArray())
            {
                dialog.Close();
            }

            owner.Close();
            LlamaCppServerManager.FolderOverride = originalFolder;
            LlamaCppServerManager.ExecutableOverride = originalExecutable;
            Directory.Delete(folder, true);
        }
    }
}
