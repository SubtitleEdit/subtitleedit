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
/// SE 4 semantics of the video offset: the rows hold video-relative time codes, the grid shows
/// and a save writes "time code + offset". "Keep existing time codes" decides whether an offset
/// change moves the file (unchecked: the rows stay, the file gets the offset - a modification) or
/// the rows (checked: the file stays as it is - not a modification). A file reopened from the
/// recent list, whose time codes carry the remembered offset, gets it taken off the rows again.
/// </summary>
public class VideoOffsetSaveTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public VideoOffsetSaveTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        Se.Settings.General.CurrentVideoOffsetInMs = 0;
    }

    public void Dispose()
    {
        // Global setting - never leave an offset behind for the next test.
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

    private string WriteSrt(string name, string firstStart, string firstEnd)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName,
            "1" + Environment.NewLine +
            firstStart + " --> " + firstEnd + Environment.NewLine +
            "Line one" + Environment.NewLine +
            Environment.NewLine +
            "2" + Environment.NewLine +
            "09:59:59,000 --> 10:00:00,000" + Environment.NewLine +
            "Line two" + Environment.NewLine);
        return fileName;
    }

    private async Task<(MainViewModel Vm, string FileName)> OpenAsync(string firstStart, string firstEnd)
    {
        var (window, vm) = ShowEmptyMainWindow();
        var fileName = WriteSrt("offset.srt", firstStart, firstEnd);
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.False(vm.HasChanges());
        return (vm, fileName);
    }

    private static void Invoke(MainViewModel vm, string method, params object[] args)
    {
        typeof(MainViewModel)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, args);
    }

    private static void ApplyOffset(MainViewModel vm, TimeSpan offset, bool keepTimeCodes) =>
        Invoke(vm, "ApplyVideoOffset", offset, false, keepTimeCodes);

    private static void ResetOffset(MainViewModel vm, bool keepTimeCodes) =>
        Invoke(vm, "ResetVideoOffset", keepTimeCodes);

    private static void SetLoading(MainViewModel vm, bool value) =>
        typeof(MainViewModel)
            .GetField("_loading", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(vm, value);

    private static RecentFile? RecentEntryFor(string fileName) =>
        Se.Settings.File.RecentFiles.FirstOrDefault(r => r.SubtitleFileName == fileName);

    private static TimeSpan FirstRowStart(MainViewModel vm) => vm.Subtitles[0].StartTime;

    private static double FirstSavedStartMs(MainViewModel vm) => vm.GetSaveSubtitle().Paragraphs[0].StartTime.TotalMilliseconds;

    private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

    [AvaloniaFact]
    public async Task AddingAnOffset_KeepsTheRowsAndMovesTheFile()
    {
        // A video-relative file that should carry the burned-in time code: the subtitles keep
        // matching the video (rows unchanged), the file gets the offset, and that is a change.
        var (vm, _) = await OpenAsync("00:00:01,000", "00:00:02,000");

        ApplyOffset(vm, OneHour, keepTimeCodes: false);

        Assert.Equal(OneSecond, FirstRowStart(vm));
        Assert.Equal((OneHour + OneSecond).TotalMilliseconds, FirstSavedStartMs(vm));
        Assert.True(vm.HasChanges());
    }

    [AvaloniaFact]
    public async Task KeepingExistingTimeCodes_MovesTheRowsAndLeavesTheFileAlone()
    {
        // A file that already starts at 01:00:01 (#11637): lining the video up must not touch
        // what gets saved, so the rows move the other way and nothing is modified.
        var (vm, _) = await OpenAsync("01:00:01,000", "01:00:02,000");

        ApplyOffset(vm, OneHour, keepTimeCodes: true);

        Assert.Equal(OneSecond, FirstRowStart(vm));
        Assert.Equal((OneHour + OneSecond).TotalMilliseconds, FirstSavedStartMs(vm));
        Assert.False(vm.HasChanges());

        // Reset with the box still checked: the file is still what it was.
        ResetOffset(vm, keepTimeCodes: true);

        Assert.Equal(OneHour + OneSecond, FirstRowStart(vm));
        Assert.Equal((OneHour + OneSecond).TotalMilliseconds, FirstSavedStartMs(vm));
        Assert.False(vm.HasChanges());
    }

    [AvaloniaFact]
    public async Task ResetWithoutKeepingTimeCodes_TakesTheOffsetOutOfTheFileAgain()
    {
        var (vm, _) = await OpenAsync("00:00:01,000", "00:00:02,000");
        ApplyOffset(vm, OneHour, keepTimeCodes: false);
        Assert.True(vm.HasChanges());

        ResetOffset(vm, keepTimeCodes: false);

        Assert.Equal(OneSecond, FirstRowStart(vm));
        Assert.Equal(OneSecond.TotalMilliseconds, FirstSavedStartMs(vm));
        Assert.False(vm.HasChanges());
    }

    [AvaloniaFact]
    public async Task ApplyingTwice_IsAgainstTheOffsetInForce()
    {
        var (vm, _) = await OpenAsync("00:00:01,000", "00:00:02,000");

        ApplyOffset(vm, OneHour, keepTimeCodes: false);
        ApplyOffset(vm, TimeSpan.FromHours(10), keepTimeCodes: false);

        Assert.Equal(OneSecond, FirstRowStart(vm));
        Assert.Equal((TimeSpan.FromHours(10) + OneSecond).TotalMilliseconds, FirstSavedStartMs(vm));
    }

    [AvaloniaFact]
    public async Task Save_WritesTheOffsetIntoTheFile()
    {
        var (vm, fileName) = await OpenAsync("00:00:01,000", "00:00:02,000");
        ApplyOffset(vm, OneHour, keepTimeCodes: false);

        var saved = await (Task<bool>)typeof(MainViewModel)
            .GetMethod("SaveSubtitle", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, new object[] { false })!;

        Assert.True(saved);
        Assert.Contains("01:00:01,000 --> 01:00:02,000", await File.ReadAllTextAsync(fileName));
        Assert.Equal(OneSecond, FirstRowStart(vm)); // the rows are not shifted by saving
        Assert.False(vm.HasChanges());
    }

    [AvaloniaFact]
    public async Task ReopeningWithARememberedOffset_TakesItOffTheRowsAndStaysClean()
    {
        // The file on disk carries the offset (it was saved with it); the remembered offset
        // splits it back into video-relative rows plus offset, without looking modified.
        var (vm, fileName) = await OpenAsync("01:00:01,000", "01:00:02,000");

        Invoke(vm, "SetRecentFileProperties", new RecentFile
        {
            SubtitleFileName = fileName,
            VideoOffsetInMs = (long)OneHour.TotalMilliseconds,
        });

        Assert.Equal(OneSecond, FirstRowStart(vm));
        Assert.Equal((OneHour + OneSecond).TotalMilliseconds, FirstSavedStartMs(vm));
        Assert.False(vm.HasChanges());
    }

    [AvaloniaFact]
    public async Task ReopeningWithARememberedOffset_WritesTheOffsetBackToTheRecentFile()
    {
        // The open that precedes the restore zeroes the offset (ResetSubtitle) and then persists
        // the recent entry twice while it is still zero - when the video opens and at the end of
        // SubtitleOpen. So by the time the offset is restored, the remembered value is already
        // gone from disk; the restore has to write it back or it survives only in memory.
        var (vm, fileName) = await OpenAsync("01:00:01,000", "01:00:02,000");
        SetLoading(vm, false);

        // What the open leaves behind: the entry, with the offset still zeroed.
        Invoke(vm, "AddToRecentFiles", false, (int?)0);
        Assert.Equal(0, RecentEntryFor(fileName)!.VideoOffsetInMs);

        Invoke(vm, "SetRecentFileProperties", new RecentFile
        {
            SubtitleFileName = fileName,
            SelectedLine = 1,
            VideoOffsetInMs = (long)OneHour.TotalMilliseconds,
        });

        var entry = RecentEntryFor(fileName);
        Assert.NotNull(entry);
        Assert.Equal((long)OneHour.TotalMilliseconds, entry!.VideoOffsetInMs);

        // The restored line is passed explicitly - the selection is applied through a dispatcher
        // post, so reading it back off the view model could persist 0 instead.
        Assert.Equal(1, entry.SelectedLine);
    }
}
