using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// Restoring a session hands the remembered line's start time to the video player so the file
/// opens where the user was, instead of opening at 0:00 and seeking a few hundred milliseconds
/// later - which showed the start of the video and then a visible jump (issue #13329).
/// A remembered row 0 is not a restored position though, it is just where the grid always lands
/// on open, so that case must still open at the beginning (issues #13191 / #12898).
/// </summary>
public class RestoredVideoStartPositionTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public RestoredVideoStartPositionTests()
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

    private string WriteSrt(string name)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName,
            "1" + Environment.NewLine +
            "00:00:01,000 --> 00:00:02,000" + Environment.NewLine +
            "Line one" + Environment.NewLine +
            Environment.NewLine +
            "2" + Environment.NewLine +
            "00:01:30,500 --> 00:01:32,000" + Environment.NewLine +
            "Line two" + Environment.NewLine +
            Environment.NewLine +
            "3" + Environment.NewLine +
            "00:02:05,000 --> 00:02:06,000" + Environment.NewLine +
            "Line three" + Environment.NewLine);
        return fileName;
    }

    private static double GetStartPosition(MainViewModel vm, int? selectedSubtitleIndex) =>
        (double)typeof(MainViewModel)
            .GetMethod("GetRestoredVideoStartPositionSeconds", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, new object?[] { selectedSubtitleIndex })!;

    private async Task<MainViewModel> OpenThreeLineFileAsync()
    {
        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteSrt("restore.srt"), skipLoadVideo: true);
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        Assert.Equal(3, vm.Subtitles.Count);
        return vm;
    }

    [AvaloniaFact]
    public async Task RestoredLineOpensTheVideoAtThatLinesStartTime()
    {
        var vm = await OpenThreeLineFileAsync();

        Assert.Equal(90.5, GetStartPosition(vm, 1));
        Assert.Equal(125, GetStartPosition(vm, 2));
    }

    [AvaloniaFact]
    public async Task FirstRowAndNoRememberedLineOpenTheVideoAtTheBeginning()
    {
        var vm = await OpenThreeLineFileAsync();

        Assert.Equal(0, GetStartPosition(vm, 0));
        Assert.Equal(0, GetStartPosition(vm, null));
    }

    [AvaloniaFact]
    public async Task StaleRememberedLineOpensTheVideoAtTheBeginning()
    {
        var vm = await OpenThreeLineFileAsync();

        // The file shrank since the line was remembered - no position to restore.
        Assert.Equal(0, GetStartPosition(vm, 9));
    }
}
