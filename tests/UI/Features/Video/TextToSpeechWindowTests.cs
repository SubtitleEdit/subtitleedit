using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// Construction smoke test for the text-to-speech window: its layout is built entirely in code,
/// so a bad grid index or binding only surfaces when the window is instantiated - which no other
/// test (and no plain app-start smoke run) does.
/// </summary>
public class TextToSpeechWindowTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    // WindowService only touches the provider when it creates a child window, which this
    // construction test never does.
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private TextToSpeechWindow BuildWindow()
    {
        var vm = new TextToSpeechViewModel(
            new TtsDownloadService(new HttpClient()),
            new WindowService(new NullServiceProvider()),
            new FileHelper(),
            new FolderHelper());
        var window = new TextToSpeechWindow(vm);
        _windows.Add(window);
        return window;
    }

    private static IEnumerable<Button> AllButtons(TextToSpeechWindow window)
    {
        return window.GetLogicalDescendants().OfType<Button>();
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void GenerateButton_IsTheAccentPrimaryAction()
    {
        var window = BuildWindow();

        var accentButtons = AllButtons(window).Where(b => b.Classes.Contains("accent")).ToList();

        Assert.Single(accentButtons);
    }
}
