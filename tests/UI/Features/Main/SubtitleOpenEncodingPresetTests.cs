using System.Text;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// "Target encoding auto-selected on open" (#9382) presets the encoding that Save will use; the
/// file itself is not touched until a save. The report was about a file that already was UTF-8 -
/// the old code skipped exactly those - so both source BOM states are covered against all three
/// values of the row: the encoding picked on open, the bytes on disk right after opening, and the
/// BOM that the following save writes.
/// </summary>
public class SubtitleOpenEncodingPresetTests : IDisposable
{
    private const string Srt =
        "1\r\n00:00:01,000 --> 00:00:02,000\r\nLine one\r\n\r\n" +
        "2\r\n00:00:03,000 --> 00:00:04,000\r\nLine two\r\n";

    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public enum Setting
    {
        AsIs,
        WithBom,
        WithoutBom,
    }

    public SubtitleOpenEncodingPresetTests()
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

    private string WriteSrt(bool withBom)
    {
        var fileName = Path.Combine(_tempDirectory, withBom ? "source-with-bom.srt" : "source-without-bom.srt");
        File.WriteAllText(fileName, Srt, new UTF8Encoding(withBom));
        return fileName;
    }

    private static bool HasUtf8Bom(byte[] bytes)
    {
        return bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
    }

    [AvaloniaTheory]
    // source file already has a BOM, row value
    [InlineData(false, Setting.AsIs)]
    [InlineData(true, Setting.AsIs)]
    [InlineData(false, Setting.WithBom)]
    [InlineData(true, Setting.WithBom)]
    [InlineData(false, Setting.WithoutBom)]
    [InlineData(true, Setting.WithoutBom)]
    public async Task OpeningPresetsTheChosenVariantAndSaveWritesItsBom(bool sourceHasBom, Setting setting)
    {
        using var _ = new SettingsScope("General.AutoConvertToUtf8", "General.AutoConvertToUtf8WithoutBom");
        Se.Settings.General.AutoConvertToUtf8 = setting != Setting.AsIs;
        Se.Settings.General.AutoConvertToUtf8WithoutBom = setting == Setting.WithoutBom;

        var fileName = WriteSrt(sourceHasBom);
        var bytesBeforeOpen = File.ReadAllBytes(fileName);

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(fileName, skipLoadVideo: true);
        Settle(window);

        var expectedEncoding = setting switch
        {
            Setting.WithBom => TextEncoding.Utf8WithBom,
            Setting.WithoutBom => TextEncoding.Utf8WithoutBom,
            // "As is" leaves the detected variant, which is the one the file already carries.
            _ => sourceHasBom ? TextEncoding.Utf8WithBom : TextEncoding.Utf8WithoutBom,
        };

        Assert.Equal(expectedEncoding, vm.SelectedEncoding!.DisplayName);

        // Preset only: opening must not rewrite the file, and must not make it look edited.
        Assert.Equal(bytesBeforeOpen, File.ReadAllBytes(fileName));
        Assert.False(vm.HasChanges());

        await vm.CommandFileSaveCommand.ExecuteAsync(null);

        var bytesAfterSave = File.ReadAllBytes(fileName);
        var expectedBom = expectedEncoding == TextEncoding.Utf8WithBom;
        Assert.Equal(expectedBom, HasUtf8Bom(bytesAfterSave));

        // Same text either way - only the BOM may have changed.
        var text = new UTF8Encoding(false).GetString(bytesAfterSave).TrimStart('\uFEFF');
        Assert.Contains("Line one", text);
        Assert.Contains("Line two", text);
    }
}
