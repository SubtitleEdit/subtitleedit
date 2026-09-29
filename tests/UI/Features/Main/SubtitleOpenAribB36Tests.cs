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
/// File > Open reads ARIB STD-B36 caption files (.1hd, .2hd, .1sd, .2sd) again, like SE 4 did.
/// </summary>
public class SubtitleOpenAribB36Tests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenAribB36Tests()
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

    /// <summary>Builds an ARIB B36 file with one page per (start, end, alphanumeric text) cue.</summary>
    private static byte[] MakeAribB36(params (string Start, string End, string Text)[] cues)
    {
        const int blockSize = 256;
        var buffer = new byte[Math.Max(3072, blockSize * (3 + cues.Length) + blockSize)];
        Encoding.ASCII.GetBytes("DCAPTION").CopyTo(buffer, 0);

        for (var i = 0; i < cues.Length; i++)
        {
            var block = blockSize * (3 + i);
            buffer[block + 2] = 0;
            buffer[block + 3] = 250; // block length
            buffer[block + 4] = 0x2a; // page management information

            var index = block + 5;
            const int pageManagementLength = 120;
            buffer[index++] = 0;
            buffer[index++] = pageManagementLength;
            Encoding.ASCII.GetBytes("T").CopyTo(buffer, index + 9); // timing unit: milliseconds
            Encoding.ASCII.GetBytes(cues[i].Start).CopyTo(buffer, index + 10); // HHMMSSmmm
            Encoding.ASCII.GetBytes(cues[i].End).CopyTo(buffer, index + 19);
            index += pageManagementLength;

            buffer[index++] = 0x3a; // caption text page management data
            const int captionTextPageManagementLength = 20;
            buffer[index++] = 0;
            buffer[index++] = captionTextPageManagementLength;
            Encoding.ASCII.GetBytes("eng").CopyTo(buffer, index + 14);
            index += captionTextPageManagementLength;

            buffer[index++] = 0x4a; // caption text data
            var textData = new List<byte> { 0x9b }; // CSI
            textData.AddRange(Encoding.ASCII.GetBytes("170;30 a"));
            textData.Add(0x0e); // LS1: alphanumeric set into GL
            textData.Add(0x89); // MSZ: middle size, so the letters decode half width
            textData.AddRange(Encoding.ASCII.GetBytes(cues[i].Text));
            var unitLength = 5 + textData.Count;
            buffer[index++] = 0;
            buffer[index++] = (byte)(15 + unitLength);
            buffer[index + 14] = (byte)unitLength; // data unit loop length
            var unit = index + 15;
            buffer[unit++] = 0x1f; // unit separator
            buffer[unit++] = 0x20; // statement body (text)
            buffer[unit++] = 0;
            buffer[unit++] = 0;
            buffer[unit++] = (byte)textData.Count;
            textData.ToArray().CopyTo(buffer, unit);
        }

        return buffer;
    }

    [AvaloniaFact]
    public async Task AribB36FileOpensWithItsCaptions()
    {
        var fileName = Path.Combine(_tempDirectory, "captions.2hd");
        File.WriteAllBytes(fileName, MakeAribB36(("000001000", "000003000", "Hello"), ("000004000", "000006000", "World")));

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        Settle(window);

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Hello", vm.Subtitles[0].Text);
        Assert.Equal("World", vm.Subtitles[1].Text);

        // ARIB has no writer, so the dropdown keeps a format that can save.
        Assert.IsNotType<AribB36>(vm.SelectedSubtitleFormat);
    }
}
