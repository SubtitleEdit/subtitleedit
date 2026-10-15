using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// Layout tests for the "Generate video with transparent subtitles" window. Like the burn-in
/// window, the style column scrolls on its own and the preview gives up height first, while the
/// progress bar and the button bar stay outside both, so a screen too short for the dialog never
/// clips "Generate" off the bottom (issues #13904, #14360).
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

    private static Control RootRow(TransparentSubtitlesWindow window, int row) =>
        Assert.IsType<Grid>(window.Content).Children.OfType<Control>().Single(c => Grid.GetRow(c) == row);

    private static double Bottom(Visual visual, Visual relativeTo) =>
        visual.TranslatePoint(new Point(0, visual.Bounds.Height), relativeTo)?.Y ?? double.MaxValue;

    [AvaloniaFact]
    public void ButtonRow_StaysReachable_OnAShortScreen()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<TransparentSubtitlesViewModel>(window.DataContext);
        vm.IsGenerating = true; // the progress bar only shows while generating
        window.Show();

        // A 1366x768 laptop at 125% scaling leaves about 560 DIPs of client height for a
        // maximized window; the button row must still end inside the window.
        window.MinHeight = 0;
        window.Height = 560;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var progress = RootRow(window, 1);
        var buttons = RootRow(window, 2);
        Assert.True(progress.IsVisible);
        Assert.True(
            Bottom(buttons, window) <= window.ClientSize.Height + 1.5,
            $"Buttons bottom ({Bottom(buttons, window):0.#}) is clipped by the window height ({window.ClientSize.Height:0.#}).");
        Assert.True(Bottom(progress, window) <= buttons.TranslatePoint(new Point(0, 0), window)!.Value.Y + 1.5,
            "The progress bar overlaps the button bar.");

        // The style column gives way by scrolling, not by being clipped
        var scroller = window.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Content is StackPanel);
        Assert.True(Bottom(scroller, window) <= progress.TranslatePoint(new Point(0, 0), window)!.Value.Y + 1.5,
            "The style column paints over the progress bar.");
    }

    [AvaloniaFact]
    public void Preview_FillsTheWindow_WhenThereIsRoom()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<TransparentSubtitlesViewModel>(window.DataContext);
        vm.SingleModeCommand.Execute(null);
        window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var preview = Assert.IsAssignableFrom<Control>(vm.VideoPlayerControl!.Parent);
        var before = preview.Bounds.Height;

        window.Height += 200;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.True(
            preview.Bounds.Height >= before + 190,
            $"Preview did not grow with the window ({before:0.#} -> {preview.Bounds.Height:0.#}).");
    }

    [AvaloniaFact]
    public void BatchMode_ShowsTheJobListInPlaceOfThePreview()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<TransparentSubtitlesViewModel>(window.DataContext);
        window.Show();
        var preview = Assert.IsAssignableFrom<Control>(vm.VideoPlayerControl!.Parent);
        var jobList = window.GetLogicalDescendants().OfType<Control>().Single(c => c.GetType().Name == "TableView");

        vm.SingleModeCommand.Execute(null);
        Assert.True(preview.IsEffectivelyVisible);
        Assert.False(jobList.IsEffectivelyVisible);

        vm.BatchModeCommand.Execute(null);
        Assert.False(preview.IsEffectivelyVisible);
        Assert.True(jobList.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void AlignmentGrid_SetsTheAlignment()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<TransparentSubtitlesViewModel>(window.DataContext);
        window.Show();
        var anchors = window.GetLogicalDescendants().OfType<RadioButton>().Where(r => r.Classes.Contains("anchor")).ToList();
        Assert.Equal(9, anchors.Count);

        anchors[0].IsChecked = true; // top left
        Assert.Equal("7", vm.SelectedFontAlignment.Code);

        vm.SelectedFontAlignment = vm.FontAlignments.First(a => a.Code == "3");
        Assert.True(anchors[8].IsChecked);
        Assert.False(anchors[0].IsChecked);
    }
}
