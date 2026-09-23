using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Opening an audio file whose tags carry LRC lyrics loads the lyrics into the grid - but the
/// audio file itself must never become the save target. Issue #15213: Ctrl+S (or auto-save) wrote
/// the LRC text over the .opus, destroying the audio.
/// </summary>
public class SubtitleOpenAudioLyricsTests : IDisposable
{
    // 0.1 s of silent Opus with a LYRICS tag holding three LRC lines (made with ffmpeg).
    private const string OpusWithLyricsBase64 =
        "T2dnUwACAAAAAAAAAAAAAAAAAAAAAAIotXIBE09wdXNIZWFkAQE4AYC7AAAAAABPZ2dTAAAAAAAAAAAAAAAAAAABAAAAEir80AF6" +
        "T3B1c1RhZ3MGAAAAZmZtcGVnAgAAABQAAABlbmNvZGVyPUxhdmMgbGlib3B1c0gAAABMWVJJQ1M9WzAwOjAxLjAwXUZpcnN0IGxp" +
        "bmUNClswMDowMy4wMF1TZWNvbmQgbGluZQ0KWzAwOjA1LjAwXVRoaXJkIGxpbmVPZ2dTAAT4EwAAAAAAAAAAAAACAAAAGv9m6AYH" +
        "BgYGBgYIC+S5oLyECAfGsw7GCAfGsw7GCAfGsw7GCAfGsw7GCAfGsw7G";

    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenAudioLyricsTests()
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

    private static T GetPrivateField<T>(MainViewModel vm, string name) =>
        (T)typeof(MainViewModel).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(vm)!;

    [AvaloniaFact]
    public async Task LyricsFromAudioFile_AreLoaded_AndSaveNeverTargetsTheAudioFile()
    {
        var audioFileName = Path.Combine(_tempDirectory, "song.opus");
        var audioBytes = Convert.FromBase64String(OpusWithLyricsBase64);
        File.WriteAllBytes(audioFileName, audioBytes);

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(audioFileName, skipLoadVideo: true);
        Settle(window);

        // From the tag, not from the raw bytes - those lose the first line to "LYRICS=".
        Assert.Equal(new[] { "First line", "Second line", "Third line" }, vm.Subtitles.Select(p => p.Text));

        Assert.Equal(Path.ChangeExtension(audioFileName, ".lrc"), GetPrivateField<string>(vm, "_subtitleFileName"));
        Assert.True(GetPrivateField<bool>(vm, "_converted"));

        // Auto-save goes the same way as Ctrl+S but can't pop "Save as": it has to skip, not
        // write the LRC text over the audio file.
        var saveTask = (Task<bool>)typeof(MainViewModel)
            .GetMethod("SaveSubtitle", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, new object[] { true })!;
        Assert.False(await saveTask);

        Assert.Equal(audioBytes, File.ReadAllBytes(audioFileName));
        Assert.False(File.Exists(Path.ChangeExtension(audioFileName, ".lrc")));
    }
}
