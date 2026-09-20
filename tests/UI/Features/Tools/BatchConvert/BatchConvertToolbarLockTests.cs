using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using System.Windows.Input;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// A run converts a snapshot of the file list, so files removed mid-run were still converted and
/// files added mid-run were silently skipped (#15116). The Add/Remove/Clear toolbar buttons are
/// therefore locked while converting, like the Delete key and the context menu already were.
/// </summary>
public class BatchConvertToolbarLockTests
{
    [AvaloniaFact]
    public void FileListButtons_AreEnabledOnOpen_AndLockedWhileControlsAreDisabled()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<BatchConvertViewModel>();
        var window = new BatchConvertWindow(vm);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var commands = new ICommand[]
        {
            vm.AddFilesCommand,
            vm.AddFolderCommand,
            vm.AddFolderRecursiveCommand,
            vm.RemoveSelectedFilesCommand,
            vm.ClearAllFilesCommand,
        };
        var buttons = window.GetVisualDescendants()
            .OfType<Button>()
            .Where(b => b.Command != null && commands.Contains(b.Command))
            .ToList();
        Assert.Equal(commands.Length, buttons.Count);

        Assert.All(buttons, b => Assert.True(b.IsEnabled));

        vm.AreControlsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        Assert.All(buttons, b => Assert.False(b.IsEnabled));

        vm.AreControlsEnabled = true;
        Dispatcher.UIThread.RunJobs();
        Assert.All(buttons, b => Assert.True(b.IsEnabled));

        window.Close();
    }
}
