using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// The Excel dialogue list has the time codes a save writes (grid times plus the video offset)
/// and only the working lines.
/// </summary>
public class DialogueListExportRowsTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public DialogueListExportRowsTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        Se.Settings.General.CurrentVideoOffsetInMs = 0;
    }

    public void Dispose()
    {
        Se.Settings.General.CurrentVideoOffsetInMs = 0;

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
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }

    private static void ApplyOffset(MainViewModel vm, TimeSpan offset, bool keepTimeCodes) =>
        typeof(MainViewModel)
            .GetMethod("ApplyVideoOffset", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, new object[] { offset, false, keepTimeCodes });

    [AvaloniaFact]
    public async Task Rows_HaveTheVideoOffsetAndActor()
    {
        var (window, vm) = ShowEmptyMainWindow();
        var fileName = Path.Combine(_tempDirectory, "dialogue.srt");
        await File.WriteAllTextAsync(fileName,
            "1" + Environment.NewLine +
            "00:00:01,000 --> 00:00:02,000" + Environment.NewLine +
            "Line one" + Environment.NewLine + Environment.NewLine +
            "2" + Environment.NewLine +
            "00:00:03,000 --> 00:00:04,000" + Environment.NewLine +
            "Line two" + Environment.NewLine);
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        vm.Subtitles[1].Actor = "Anna";
        ApplyOffset(vm, TimeSpan.FromHours(1), keepTimeCodes: false);

        var rows = vm.GetDialogueListRows();

        Assert.Equal(2, rows.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(3601000), rows[0].Start);
        Assert.Equal(TimeSpan.FromMilliseconds(3604000), rows[1].End);
        Assert.Equal("Line two", rows[1].Text);
        Assert.Equal("Anna", rows[1].Actor);
        Assert.Equal(2, rows[1].Number);
    }
}
