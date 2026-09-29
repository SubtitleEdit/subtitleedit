using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System.IO.Compression;
using System.Text;

namespace UITests.Features.Main;

/// <summary>
/// File > Open reads the text clips of an Adobe Premiere project (.prproj, gzipped xml) again,
/// like SE 4 did.
/// </summary>
public class SubtitleOpenPremiereProjectTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenPremiereProjectTests()
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

    // One "Text" clip from 4 to 6 seconds - Premiere counts 254016000000 ticks per second.
    private const string ProjectXml = """
        <PremiereData Version="3">
          <VideoComponentChain ObjectID="10">
            <ComponentChain>
              <Components>
                <Component ObjectRef="20"/>
              </Components>
            </ComponentChain>
          </VideoComponentChain>
          <VideoClipTrackItem ObjectID="30">
            <ClipTrackItem>
              <ComponentOwner>
                <Components ObjectRef="10"/>
              </ComponentOwner>
              <TrackItem>
                <Start>1016064000000</Start>
                <End>1524096000000</End>
              </TrackItem>
            </ClipTrackItem>
          </VideoClipTrackItem>
          <VideoFilterComponent ObjectID="20">
            <Component>
              <DisplayName>Text</DisplayName>
              <InstanceName>Hello title</InstanceName>
            </Component>
          </VideoFilterComponent>
        </PremiereData>
        """;

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PremiereProjectOpensWithItsTextClips(bool gzipped)
    {
        var fileName = Path.Combine(_tempDirectory, "project.prproj");
        var bytes = Encoding.UTF8.GetBytes(ProjectXml);
        if (gzipped)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }

            bytes = output.ToArray();
        }

        File.WriteAllBytes(fileName, bytes);

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        Settle(window);

        Assert.Single(vm.Subtitles);
        Assert.Equal("Hello title", vm.Subtitles[0].Text);
        Assert.Equal(4000, vm.Subtitles[0].StartTime.TotalMilliseconds, 1.0);
        Assert.Equal(6000, vm.Subtitles[0].EndTime.TotalMilliseconds, 1.0);
    }
}
