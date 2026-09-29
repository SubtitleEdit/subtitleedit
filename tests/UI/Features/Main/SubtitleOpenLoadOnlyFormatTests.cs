using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System.Text;

namespace UITests.Features.Main;

/// <summary>
/// File > Open tries the load-only text formats (SubtitleFormat.GetTextOtherFormats) again, like
/// SE 4 did as its last resort - without that, a file only one of them understands ended up in
/// the generic importer or was reported as an unknown format.
/// </summary>
public class SubtitleOpenLoadOnlyFormatTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenLoadOnlyFormatTests()
    {
        // The binary-format probes before the fallback use code pages (850, 1252) that Program
        // registers at startup.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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

    [AvaloniaFact]
    public async Task JsonTypeOnlyLoad1FileOpensWithItsLines()
    {
        var fileName = Path.Combine(_tempDirectory, "words.json");
        File.WriteAllText(fileName,
            "{\"words\":[{\"time\":1.0,\"duration\":0.5,\"name\":\"Hello\"},{\"time\":1.5,\"duration\":0.2,\"name\":\".\"}," +
            "{\"time\":3.0,\"duration\":0.6,\"name\":\"World\"}]}");

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        Settle(window);

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Hello.", vm.Subtitles[0].Text);
        Assert.Equal("World", vm.Subtitles[1].Text);

        // The load-only format has no writer, so the dropdown keeps a format that can save.
        Assert.IsNotType<JsonTypeOnlyLoad1>(vm.SelectedSubtitleFormat);
    }
}
