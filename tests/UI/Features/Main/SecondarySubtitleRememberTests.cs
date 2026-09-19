using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// The second subtitle file (Video menu) is kept on the recent-file entry of the subtitle it was
/// shown with, and comes back when that subtitle is opened again - if "Remember second subtitle
/// file" is on (#15044).
/// </summary>
public class SecondarySubtitleRememberTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;
    private readonly bool _oldRememberFile;

    public SecondarySubtitleRememberTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _oldRememberFile = Se.Settings.Video.SecondarySubtitleRememberFile;
    }

    public void Dispose()
    {
        Se.Settings.Video.SecondarySubtitleRememberFile = _oldRememberFile;

        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private MainViewModel ShowEmptyMainWindow()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);

        // The recent-file writes are skipped while the window still counts as loading.
        typeof(MainViewModel)
            .GetField("_loading", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(vm, false);
        return vm;
    }

    private string WriteSrt(string name, string text)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName,
            "1" + Environment.NewLine +
            "00:00:01,000 --> 00:00:02,000" + Environment.NewLine +
            text + Environment.NewLine);
        return fileName;
    }

    private static string? SecondaryFileName(MainViewModel vm) =>
        (string?)typeof(MainViewModel)
            .GetField("_subtitleSecondaryFileName", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm);

    private static void RememberSecondary(string fileName, string secondaryFileName) =>
        Se.Settings.File.AddToRecentFiles(fileName, string.Empty, string.Empty, 0, string.Empty, 0, false, -1, secondaryFileName);

    private static RecentFile? RecentEntryFor(string fileName) =>
        Se.Settings.File.RecentFiles.FirstOrDefault(r => r.SubtitleFileName == fileName);

    [AvaloniaFact]
    public async Task Opening_RestoresTheRememberedSecondSubtitle_AndKeepsItOnTheEntry()
    {
        Se.Settings.Video.SecondarySubtitleRememberFile = true;
        var vm = ShowEmptyMainWindow();
        var fileName = WriteSrt("main.srt", "Main");
        var secondaryFileName = WriteSrt("second.srt", "Second");
        RememberSecondary(fileName, secondaryFileName);

        await vm.SubtitleOpen(fileName, skipLoadVideo: true);

        Assert.Equal(secondaryFileName, SecondaryFileName(vm));
        Assert.True(vm.IsSubtitleSecondaryVisible);

        // The open rewrites the entry - the second subtitle must not be dropped by that.
        Assert.Equal(secondaryFileName, RecentEntryFor(fileName)!.SubtitleFileNameSecondary);
    }

    [AvaloniaFact]
    public async Task Opening_LeavesTheSecondSubtitleAlone_WhenTheSettingIsOff()
    {
        Se.Settings.Video.SecondarySubtitleRememberFile = false;
        var vm = ShowEmptyMainWindow();
        var fileName = WriteSrt("main.srt", "Main");
        RememberSecondary(fileName, WriteSrt("second.srt", "Second"));

        await vm.SubtitleOpen(fileName, skipLoadVideo: true);

        Assert.Null(SecondaryFileName(vm));
        Assert.False(vm.IsSubtitleSecondaryVisible);
    }

    [AvaloniaFact]
    public async Task Opening_SkipsASecondSubtitleThatIsGone()
    {
        Se.Settings.Video.SecondarySubtitleRememberFile = true;
        var vm = ShowEmptyMainWindow();
        var fileName = WriteSrt("main.srt", "Main");
        RememberSecondary(fileName, Path.Combine(_tempDirectory, "missing.srt"));

        await vm.SubtitleOpen(fileName, skipLoadVideo: true);

        Assert.Null(SecondaryFileName(vm));
        Assert.Single(vm.Subtitles);
    }
}
