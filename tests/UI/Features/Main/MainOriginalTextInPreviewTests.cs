using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// SE 4 parity for "toggle translation and original in video/audio preview" (#14252): while it is
/// on, the video preview and the waveform show the original text instead of the translation. It is
/// session state, and only ever on while an original is loaded - a stale "on" without an original
/// would leave both previews blank with no visible way back.
/// </summary>
public class MainOriginalTextInPreviewTests
{
    [AvaloniaFact]
    public void Toggle_WithOriginalShown_PreviewSubtitleUsesTheOriginalText()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel een", "Source line one", 0, 2000);
            AddLine(vm, "Vertaalde regel twee", "Source line two", 2000, 4000);
            vm.ShowColumnOriginalText = true;

            vm.ToggleOriginalTextInPreviewCommand.Execute(null);

            Assert.True(vm.ShowOriginalTextInPreview);
            Assert.Equal(
                new[] { "Source line one", "Source line two" },
                GetVideoPreviewSubtitle(vm).Paragraphs.Select(p => p.Text));

            vm.ToggleOriginalTextInPreviewCommand.Execute(null);

            Assert.False(vm.ShowOriginalTextInPreview);
            Assert.Equal(
                new[] { "Vertaalde regel een", "Vertaalde regel twee" },
                GetVideoPreviewSubtitle(vm).Paragraphs.Select(p => p.Text));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// A row the original has no line for has nothing to preview - it is left out rather than
    /// pushed to the player as an empty subtitle line.
    /// </summary>
    [AvaloniaFact]
    public void Toggle_SkipsRowsWithoutAnOriginalLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel een", "Source line one", 0, 2000);
            AddLine(vm, "Vertaalde regel twee", string.Empty, 2000, 4000);
            vm.ShowColumnOriginalText = true;

            vm.ToggleOriginalTextInPreviewCommand.Execute(null);

            var preview = GetVideoPreviewSubtitle(vm);
            Assert.Equal(2, vm.Subtitles.Count);
            Assert.Equal("Source line one", Assert.Single(preview.Paragraphs).Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Toggle_WithoutAnOriginal_StaysOnTheTranslation()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Line one", string.Empty, 0, 2000);

            vm.ToggleOriginalTextInPreviewCommand.Execute(null);

            Assert.False(vm.ShowOriginalTextInPreview);
            Assert.Equal("Line one", Assert.Single(GetVideoPreviewSubtitle(vm).Paragraphs).Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void HidingTheOriginalColumn_TurnsThePreviewBackToTheTranslation()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel een", "Source line one", 0, 2000);
            vm.ShowColumnOriginalText = true;
            vm.ToggleOriginalTextInPreviewCommand.Execute(null);
            Assert.True(vm.ShowOriginalTextInPreview);

            vm.ShowColumnOriginalText = false;

            Assert.False(vm.ShowOriginalTextInPreview);
            Assert.Equal("Vertaalde regel een", Assert.Single(GetVideoPreviewSubtitle(vm).Paragraphs).Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The waveform draws from the rows, so it needs the same flag - without this the video preview
    /// would show the original while the waveform kept showing the translation.
    /// </summary>
    [AvaloniaFact]
    public void Toggle_KeepsTheWaveformInSync()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel een", "Source line one", 0, 2000);
            vm.ShowColumnOriginalText = true;
            Assert.NotNull(vm.AudioVisualizer);

            vm.ToggleOriginalTextInPreviewCommand.Execute(null);
            Assert.True(vm.AudioVisualizer!.ShowOriginalText);

            vm.ShowColumnOriginalText = false;
            Assert.False(vm.AudioVisualizer!.ShowOriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static Subtitle GetVideoPreviewSubtitle(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "GetVideoPreviewSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("GetVideoPreviewSubtitle not found");

        return (Subtitle)method.Invoke(vm, null)!;
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

    private static void AddLine(MainViewModel vm, string text, string originalText, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
