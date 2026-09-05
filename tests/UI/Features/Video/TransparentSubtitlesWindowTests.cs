using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// Layout tests for the "Generate video with transparent subtitles" window. Like the burn-in
/// window, its settings grid sits in a ScrollViewer while the progress bar and the button row
/// stay outside it, so a screen too short for the dialog scrolls the settings instead of
/// clipping "Generate" off the bottom (issues #13904, #14360).
/// </summary>
public class TransparentSubtitlesWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private TransparentSubtitlesWindow BuildWindow()
    {
        var vm = new TransparentSubtitlesViewModel(
            new FolderHelper(),
            new FileHelper(),
            new WindowService(new NullServiceProvider()));
        var window = new TransparentSubtitlesWindow(vm);
        _windows.Add(window);
        return window;
    }

    private static Grid FindRootGrid(TransparentSubtitlesWindow window)
    {
        var progressBar = window.GetLogicalDescendants().OfType<ProgressBar>().FirstOrDefault();
        Assert.NotNull(progressBar);
        var rootGrid = (progressBar.Parent as Grid)?.Parent as Grid;
        Assert.NotNull(rootGrid);
        return rootGrid;
    }

    private static (ScrollViewer Scroller, Grid SettingsGrid) FindSettingsGrid(Grid rootGrid)
    {
        var scroller = rootGrid.Children.OfType<ScrollViewer>().FirstOrDefault(s => Grid.GetRow(s) == 0);
        Assert.NotNull(scroller);
        var settingsGrid = scroller.Content as Grid;
        Assert.NotNull(settingsGrid);
        return (scroller, settingsGrid);
    }

    [AvaloniaFact]
    public void ButtonRow_StaysReachable_OnAShortScreen()
    {
        var window = BuildWindow();
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        // A 1366x768 laptop at 125% scaling leaves about 560 DIPs of client height for a
        // maximized window; the button row must still end inside the window.
        window.SizeToContent = SizeToContent.Manual;
        window.MinHeight = 0;
        window.Height = 560;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var rootGrid = FindRootGrid(window);
        var buttonPanel = rootGrid.Children.OfType<StackPanel>().FirstOrDefault(s => Grid.GetRow(s) == 2);
        Assert.NotNull(buttonPanel);
        var buttonsBottom = buttonPanel.TranslatePoint(new Point(0, buttonPanel.Bounds.Height), window)?.Y;
        Assert.NotNull(buttonsBottom);
        Assert.True(
            buttonsBottom.Value <= window.ClientSize.Height + 1.5,
            $"Buttons bottom ({buttonsBottom.Value:0.#}) is clipped by the window height ({window.ClientSize.Height:0.#}).");

        var (scroller, _) = FindSettingsGrid(rootGrid);
        Assert.True(
            scroller.Extent.Height > scroller.Viewport.Height + 1.5,
            $"Settings area does not scroll (extent {scroller.Extent.Height:0.#}, viewport {scroller.Viewport.Height:0.#}).");
    }

    [AvaloniaFact]
    public void PreviewRow_FillsTheWindow_WhenThereIsRoom()
    {
        var window = BuildWindow();
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var (scroller, settingsGrid) = FindSettingsGrid(FindRootGrid(window));
        var before = settingsGrid.Bounds.Height;

        window.SizeToContent = SizeToContent.Manual;
        window.Height = window.ClientSize.Height + 200;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.True(
            settingsGrid.Bounds.Height >= before + 190,
            $"Settings grid did not grow with the window ({before:0.#} -> {settingsGrid.Bounds.Height:0.#}).");
        Assert.True(
            scroller.Extent.Height <= scroller.Viewport.Height + 1.5,
            $"Settings area scrolls although the window is tall enough (extent {scroller.Extent.Height:0.#}, viewport {scroller.Viewport.Height:0.#}).");
    }
}
