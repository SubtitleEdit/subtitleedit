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
public class TextToSpeechWindowTests
{
    // WindowService only touches the provider when it creates a child window, which this
    // construction test never does.
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static TextToSpeechWindow BuildWindow()
    {
        var vm = new TextToSpeechViewModel(
            new TtsDownloadService(new HttpClient()),
            new WindowService(new NullServiceProvider()),
            new FileHelper(),
            new FolderHelper());
        return new TextToSpeechWindow(vm);
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
