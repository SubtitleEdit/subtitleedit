using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System.Text;

namespace UITests.Features.Main;

/// <summary>
/// File > Open sends image-list subtitle files (each cue names an image next to the file) to
/// OCR, like SE 4 did, instead of showing the image file names as text or rejecting the file.
/// </summary>
public class SubtitleOpenImageListOcrTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenImageListOcrTests()
    {
        // The binary-format probes use code pages that Program registers at startup.
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

    /// <summary>
    /// Builds the real OCR view model the dialog would show, configures it the way the open
    /// path does, and "cancels" - so the test sees what the OCR window would have been given.
    /// </summary>
    private sealed class RecordingWindowService : IWindowService
    {
        public List<object> DialogViewModels { get; } = new();

        public T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window => throw new NotSupportedException();

        public TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class => throw new NotSupportedException();

        public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class => throw new NotSupportedException();

        public Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window => throw new NotSupportedException();

        public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Window where TViewModel : class
        {
            var vm = Locator.Services.GetRequiredService<TViewModel>();
            configureViewModel?.Invoke(vm);
            DialogViewModels.Add(vm);
            return Task.FromResult(vm);
        }

        public Task<TViewModel> ShowWithOwnerHiddenAsync<TWindow, TViewModel>(
            Window owner,
            IReadOnlyList<Window?> companions,
            Action<TViewModel>? configureViewModel = null)
            where TWindow : Window where TViewModel : class => throw new NotSupportedException();
    }

    private (Window Window, MainViewModel Vm, RecordingWindowService WindowService) ShowEmptyMainWindow()
    {
        var windowService = new RecordingWindowService();
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        services.AddSingleton<IWindowService>(windowService);
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
        return (window, vm, windowService);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private void WritePng(string name, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        bitmap.Erase(SKColors.White);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(Path.Combine(_tempDirectory, name), data.ToArray());
    }

    private async Task<(MainViewModel Vm, OcrViewModel? Ocr)> Open(string fileName)
    {
        var (window, vm, windowService) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        Settle(window);
        return (vm, windowService.DialogViewModels.OfType<OcrViewModel>().SingleOrDefault());
    }

    [AvaloniaFact]
    public async Task SonFileOpensInOcrWithItsImages()
    {
        WritePng("a_0001.png", 120, 30);
        WritePng("a_0002.png", 140, 30);
        var fileName = Path.Combine(_tempDirectory, "images.son");
        File.WriteAllText(fileName,
            "St_Title\tTest" + Environment.NewLine +
            "0001\t00:00:01:00\t00:00:03:00\ta_0001.png" + Environment.NewLine +
            "0002\t00:00:04:00\t00:00:06:00\ta_0002.png" + Environment.NewLine);

        var (vm, ocr) = await Open(fileName);

        Assert.NotNull(ocr);
        Assert.Equal(2, ocr.OcrSubtitleItems.Count);
        Assert.Equal(TimeSpan.FromSeconds(1), ocr.OcrSubtitleItems[0].StartTime);
        using var bitmap = ocr.OcrSubtitleItems[1].GetSkBitmapClean();
        Assert.Equal(140, bitmap.Width);

        // The image file names never reach the grid as text.
        Assert.Empty(vm.Subtitles);
    }

    [AvaloniaFact]
    public async Task SubRipWithImageFileNamesOpensInOcr()
    {
        var sb = new StringBuilder();
        for (var i = 1; i <= 4; i++)
        {
            WritePng($"{i:0000}.png", 100 + i, 30);
            sb.AppendLine(i.ToString());
            sb.AppendLine($"00:00:0{i * 2 - 1},000 --> 00:00:0{i * 2},000");
            sb.AppendLine($"{i:0000}.png");
            sb.AppendLine();
        }

        var fileName = Path.Combine(_tempDirectory, "images.srt");
        File.WriteAllText(fileName, sb.ToString());

        var (vm, ocr) = await Open(fileName);

        Assert.NotNull(ocr);
        Assert.Equal(4, ocr.OcrSubtitleItems.Count);
        using var bitmap = ocr.OcrSubtitleItems[3].GetSkBitmapClean();
        Assert.Equal(104, bitmap.Width);
        Assert.Empty(vm.Subtitles);
    }

    [AvaloniaFact]
    public async Task ImscImageProfileFileOpensInOcrWithItsImages()
    {
        // IMSC image profile: smpte:backgroundImage names png files next to the document.
        WritePng("1.png", 150, 30);
        WritePng("2.png", 160, 30);
        var fileName = Path.Combine(_tempDirectory, "imsc.ttml");
        File.WriteAllText(fileName, """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt xmlns="http://www.w3.org/ns/ttml" xmlns:ttp="http://www.w3.org/ns/ttml#parameter"
                xmlns:tts="http://www.w3.org/ns/ttml#styling" xmlns:smpte="http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt"
                ttp:profile="http://www.w3.org/ns/ttml/profile/imsc1/image" tts:extent="1920px 1080px" xml:lang="en">
              <head><layout><region xml:id="r1" tts:origin="10% 80%" tts:extent="80% 10%"/></layout></head>
              <body>
                <div region="r1" begin="00:00:01.000" end="00:00:02.000" smpte:backgroundImage="1.png"/>
                <div region="r1" begin="00:00:03.000" end="00:00:04.500" smpte:backgroundImage="2.png"/>
              </body>
            </tt>
            """);

        var (vm, ocr) = await Open(fileName);

        Assert.NotNull(ocr);
        Assert.Equal(2, ocr.OcrSubtitleItems.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(4500), ocr.OcrSubtitleItems[1].EndTime);
        using var bitmap = ocr.OcrSubtitleItems[1].GetSkBitmapClean();
        Assert.Equal(160, bitmap.Width);
        Assert.Empty(vm.Subtitles);
    }

    [AvaloniaFact]
    public async Task OrdinarySubRipStillOpensAsText()
    {
        var fileName = Path.Combine(_tempDirectory, "text.srt");
        File.WriteAllText(fileName,
            "1" + Environment.NewLine + "00:00:01,000 --> 00:00:02,000" + Environment.NewLine + "Hello." + Environment.NewLine + Environment.NewLine +
            "2" + Environment.NewLine + "00:00:03,000 --> 00:00:04,000" + Environment.NewLine + "Save it as logo.png" + Environment.NewLine);

        var (vm, ocr) = await Open(fileName);

        Assert.Null(ocr);
        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Save it as logo.png", vm.Subtitles[1].Text);
    }
}
