using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// The DVB teletext (.dvbttx) export has to write the same time codes a save does: grid times
/// plus the video offset. Otherwise, with an offset in force, the .dvbttx disagrees with the
/// .stl/.srt saved next to it by exactly that offset.
/// </summary>
public class DvbTeletextExportOffsetTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public DvbTeletextExportOffsetTests()
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

    private static List<Paragraph> Export(MainViewModel vm)
    {
        var writer = new ManzanitaTeletextWriter { PageNumber = 888, LanguageCode = "eng", FrameRate = 25 };
        var parser = new ManzanitaTransportStreamParser();
        using var ms = new MemoryStream(vm.GetDvbTeletextExportBytes(writer));
        parser.Parse(ms);
        return parser.GetTeletext()[888];
    }

    [AvaloniaFact]
    public async Task Export_WritesTheVideoOffsetIntoTheFile()
    {
        var (window, vm) = ShowEmptyMainWindow();
        var fileName = Path.Combine(_tempDirectory, "offset.srt");
        await File.WriteAllTextAsync(fileName,
            "1" + Environment.NewLine +
            "00:00:01,000 --> 00:00:02,000" + Environment.NewLine +
            "Line one" + Environment.NewLine);
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        Assert.Single(vm.Subtitles);
        ApplyOffset(vm, TimeSpan.FromHours(1), keepTimeCodes: false);

        // What a save would write...
        Assert.Equal(3601000, vm.GetSaveSubtitle().Paragraphs[0].StartTime.TotalMilliseconds);

        // ...is what the .dvbttx export writes.
        var paragraphs = Export(vm);
        Assert.Single(paragraphs);
        Assert.Equal(3601000, paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3602000, paragraphs[0].EndTime.TotalMilliseconds);
    }
}
