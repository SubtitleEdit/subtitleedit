using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// Opening the first file after startup leaves the commands that read the selection dead on
/// row 0 (issue #13303): the grid picks row 0 on its own (AlwaysSelected) without raising
/// SelectionChanged, and the TwoWay SelectedItem binding writes SelectedSubtitle directly -
/// so the repair in SelectAndScrollToRow, which used SelectedSubtitle as proof that the
/// selection-changed pipeline ran, was skipped and the view model's selection cache stayed
/// empty. Delete (key or context menu) then silently did nothing until the selection was
/// moved to another row.
/// </summary>
public class SubtitleOpenFirstRowDeleteTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenFirstRowDeleteTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
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

    private (Window Window, MainViewModel Vm) ShowEmptyMainWindow()
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
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private string WriteSrt(string name)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName,
            "1" + Environment.NewLine +
            "00:00:01,000 --> 00:00:02,000" + Environment.NewLine +
            "Line one" + Environment.NewLine +
            Environment.NewLine +
            "2" + Environment.NewLine +
            "00:00:03,000 --> 00:00:04,000" + Environment.NewLine +
            "Line two" + Environment.NewLine +
            Environment.NewLine +
            "3" + Environment.NewLine +
            "00:00:05,000 --> 00:00:06,000" + Environment.NewLine +
            "Line three" + Environment.NewLine);
        return fileName;
    }

    [AvaloniaFact]
    public async Task DeleteRemovesTheFirstRowOfTheFirstOpenedFile()
    {
        var (window, vm) = ShowEmptyMainWindow();
        Se.Settings.General.PromptBeforeDelete = false;

        await vm.SubtitleOpen(WriteSrt("first.srt"), skipLoadVideo: true);
        Settle(window);

        Assert.Equal(3, vm.Subtitles.Count);
        Assert.Same(vm.Subtitles[0], vm.SelectedSubtitle);

        // The user's very first action: delete the first row. Without the repair this
        // silently did nothing because the selection cache was never populated.
        await vm.DeleteSelectedLinesCommand.ExecuteAsync(null);
        Settle(window);

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Line two", vm.Subtitles[0].Text);
    }
}
