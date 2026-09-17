using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// The waveform cache file names are a hash of the video's own bytes. A read of an online-only file
/// in Dropbox or iCloud Drive waits until the file is downloaded, so hashing on the UI thread froze
/// the app when a subtitle was opened next to such a video (#14912).
/// </summary>
public class MainWaveformLoadTests
{
    [AvaloniaFact]
    public async Task LoadWaveformAndSpectrogram_VideoReadBlocks_ReturnsWithoutWaiting()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("Needs mkfifo to make a video whose read blocks.");
        }

        // A named pipe stands in for the online-only file: opening it for reading blocks until a
        // writer shows up, just as reading a cloud placeholder blocks until the download is done.
        var folder = Path.Combine(SettingsIsolationFixture.SettingsDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var videoFileName = Path.Combine(folder, "movie.mp4");
        MakeFifo(videoFileName);

        var released = 0;
        void Release()
        {
            if (Interlocked.Exchange(ref released, 1) == 0)
            {
                // Opening the write end blocks too, until the reader is there - keep it off this thread.
                _ = Task.Run(() =>
                {
                    using var writer = new FileStream(videoFileName, FileMode.Open, FileAccess.Write);
                });
            }
        }

        // Reading on the calling thread would never return; this turns that hang into a failure.
        using var watchdog = new Timer(_ => Release(), null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);

        var (window, vm) = CreateMainViewModel();
        try
        {
            var load = vm.LoadWaveformAndSpectrogram(videoFileName);
            var returnedBeforeRelease = Volatile.Read(ref released) == 0;
            var completedBeforeRelease = load.IsCompleted;
            Release();

            Assert.True(returnedBeforeRelease, "The video was read on the calling (UI) thread.");
            Assert.False(completedBeforeRelease, "The video was never read, so the test proves nothing.");

            // A pipe cannot be hashed (no length), so the load ends in a logged error, not a crash.
            await load.WaitAsync(TimeSpan.FromSeconds(30));
        }
        finally
        {
            window.SuppressSaveChangesPromptOnClose(vm);
            window.Close();
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void MakeFifo(string path)
    {
        var startInfo = new ProcessStartInfo("mkfifo") { UseShellExecute = false };
        startInfo.ArgumentList.Add(path);
        using var process = Process.Start(startInfo)!;
        process.WaitForExit();
        Assert.Equal(0, process.ExitCode);
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }
}
