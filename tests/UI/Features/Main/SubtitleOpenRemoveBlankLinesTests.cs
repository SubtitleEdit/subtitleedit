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
/// SE 4's "Remove blank lines when opening a subtitle" (issue #13588) restored: opening a file
/// with the setting on drops the lines that hold no text. Three things have to stay true beyond
/// the removal itself - the setting must be honored both ways, bookmarks (stored by line index)
/// must still land on the lines they were saved for, and the cleanup on its own must not make a
/// freshly opened file look edited.
/// </summary>
public class SubtitleOpenRemoveBlankLinesTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenRemoveBlankLinesTests()
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
        window.SuppressSaveChangesPromptOnClose(vm);
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

    /// <summary>
    /// Three cues where the middle one carries no text - the shape of the older subtitles the
    /// issue is about.
    /// </summary>
    private string WriteSrtWithBlankLine(string name)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName,
            "1" + Environment.NewLine +
            "00:00:01,000 --> 00:00:02,000" + Environment.NewLine +
            "Line one" + Environment.NewLine +
            Environment.NewLine +
            "2" + Environment.NewLine +
            "00:00:03,000 --> 00:00:04,000" + Environment.NewLine +
            Environment.NewLine +
            "3" + Environment.NewLine +
            "00:00:05,000 --> 00:00:06,000" + Environment.NewLine +
            "Line three" + Environment.NewLine);
        return fileName;
    }

    [AvaloniaFact]
    public async Task BlankLinesAreKeptWhenTheSettingIsOff()
    {
        using var _ = new SettingsScope("General.RemoveBlankLinesWhenOpening");
        Se.Settings.General.RemoveBlankLinesWhenOpening = false;

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteSrtWithBlankLine("blank-off.srt"), skipLoadVideo: true);
        Settle(window);

        Assert.Equal(3, vm.Subtitles.Count);
        Assert.Equal(string.Empty, vm.Subtitles[1].Text);
    }

    [AvaloniaFact]
    public async Task BlankLinesAreRemovedWhenTheSettingIsOn()
    {
        using var _ = new SettingsScope("General.RemoveBlankLinesWhenOpening");
        Se.Settings.General.RemoveBlankLinesWhenOpening = true;

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteSrtWithBlankLine("blank-on.srt"), skipLoadVideo: true);
        Settle(window);

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Line one", vm.Subtitles[0].Text);
        Assert.Equal("Line three", vm.Subtitles[1].Text);

        // The remaining lines are renumbered, so the grid does not open on 1, 3.
        Assert.Equal(new[] { 1, 2 }, vm.Subtitles.Select(s => s.Number).ToArray());

        // Like SE 4: the cleanup alone must not put the file in the "unsaved changes" state,
        // otherwise closing right after opening asks to save a file the user never touched.
        Assert.False(vm.HasChanges());
    }

    [AvaloniaFact]
    public async Task BookmarksStayOnTheirLineWhenBlankLinesAreRemoved()
    {
        using var _ = new SettingsScope("General.RemoveBlankLinesWhenOpening");
        Se.Settings.General.RemoveBlankLinesWhenOpening = true;

        var fileName = WriteSrtWithBlankLine("blank-bookmark.srt");

        // Bookmarks are stored by line index, so this one belongs to "Line three" - index 2 while
        // the blank line is still in the file, index 1 once it is gone.
        File.WriteAllText(fileName + ".SE.bookmarks",
            "{\"bookmarks\":[" + Environment.NewLine +
            "{\"idx\":2,\"txt\":\"third\"}" + Environment.NewLine +
            "]}" + Environment.NewLine);

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        Settle(window);

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Null(vm.Subtitles[0].Bookmark);
        Assert.Equal("third", vm.Subtitles[1].Bookmark);
    }
}
