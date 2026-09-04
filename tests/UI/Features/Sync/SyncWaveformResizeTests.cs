using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Sync;

/// <summary>
/// The sync dialogs' waveforms were pinned at 80 px (issue #14414). Both windows now host the
/// player and the waveform in a <see cref="VideoWaveformSplitGrid"/>, and remember its height.
/// </summary>
public class SyncWaveformResizeTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly double _setSyncPointHeightBefore = Se.Settings.Synchronization.SetSyncPointWaveformHeight;
    private readonly double _visualSyncHeightBefore = Se.Settings.Synchronization.VisualSyncWaveformHeight;

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        Se.Settings.Synchronization.SetSyncPointWaveformHeight = _setSyncPointHeightBefore;
        Se.Settings.Synchronization.VisualSyncWaveformHeight = _visualSyncHeightBefore;
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static List<SubtitleLineViewModel> TwoLines()
        => new()
        {
            new() { StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(3) },
            new() { StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(7) },
        };

    private static AudioVisualizer LentWaveform()
    {
        var peaks = new WavePeak2[126 * 10];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(200, -200);
        }

        return new AudioVisualizer { WavePeaks = new WavePeakData2(126, peaks) };
    }

    private T Track<T>(T window) where T : Window
    {
        _windows.Add(window);
        return window;
    }

    private static SetSyncPointViewModel MakeSetSyncPointViewModel()
        => new(new WindowService(new NullServiceProvider()), new FileHelper(), new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));

    private static VisualSyncViewModel MakeVisualSyncViewModel()
        => new(new WindowService(new NullServiceProvider()), new FileHelper(),
            new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()),
            new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));

    private static List<VideoWaveformSplitGrid> SplitsOf(Window window)
        => window.GetLogicalDescendants().OfType<VideoWaveformSplitGrid>().ToList();

    private static GridSplitter HandleOf(Grid grid) => grid.Children.OfType<GridSplitter>().Single();

    private static void SimulateDrag(VideoWaveformSplitGrid grid, double newHeight)
    {
        grid.RowDefinitions[2].Height = new GridLength(newHeight, GridUnitType.Pixel);
        HandleOf(grid).RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragDeltaEvent });
        HandleOf(grid).RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragCompletedEvent });
    }

    [AvaloniaFact]
    public void SetSyncPointWindow_WithVideoAndLentWaveform_HasADragHandleAtTheSavedHeight()
    {
        Se.Settings.Synchronization.SetSyncPointWaveformHeight = 110;
        var vm = MakeSetSyncPointViewModel();
        var lines = TwoLines();
        // A path is enough for the layout decision; nothing opens it here (the open is posted to
        // the dispatcher and this test never pumps it).
        vm.Initialize(lines, lines[0], videoFileName: "/does/not/exist.mp4", subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: LentWaveform());
        var window = Track(new SetSyncPointWindow(vm));

        // The peaks are handed over on the same posted job - flip the flag the way it does.
        vm.IsAudioVisualizerVisible = true;

        var split = Assert.Single(SplitsOf(window));
        Assert.True(HandleOf(split).IsVisible);
        Assert.True(split.RowDefinitions[2].Height.IsAbsolute);
        Assert.Equal(110, split.RowDefinitions[2].Height.Value);
        Assert.True(double.IsNaN(vm.AudioVisualizer.Height)); // the row, not the control, sets the height now
    }

    [AvaloniaFact]
    public void SetSyncPointWindow_WithoutLentWaveform_HasNoHandleAndNoWaveformRow()
    {
        var vm = MakeSetSyncPointViewModel();
        var lines = TwoLines();
        vm.Initialize(lines, lines[0], videoFileName: "/does/not/exist.mp4", subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        var window = Track(new SetSyncPointWindow(vm));

        var split = Assert.Single(SplitsOf(window));
        Assert.False(HandleOf(split).IsVisible);
        Assert.Equal(0, split.RowDefinitions[2].Height.Value);
    }

    [AvaloniaFact]
    public void SetSyncPointWindow_RemembersTheDraggedHeightOnClose()
    {
        Se.Settings.Synchronization.SetSyncPointWaveformHeight = 80;
        var vm = MakeSetSyncPointViewModel();
        var lines = TwoLines();
        vm.Initialize(lines, lines[0], videoFileName: "/does/not/exist.mp4", subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: LentWaveform());
        var window = new SetSyncPointWindow(vm);
        vm.IsAudioVisualizerVisible = true;

        SimulateDrag(SplitsOf(window).Single(), 140);
        window.Close();

        Assert.Equal(140, Se.Settings.Synchronization.SetSyncPointWaveformHeight);
    }

    [AvaloniaFact]
    public void SetSyncPointWindow_WithoutVideoButWithLentWaveform_LiftsTheVideolessHeightCap()
    {
        // The videoless cap assumes there is nothing tall to show; a lent waveform is, and it fills
        // whatever height the user gives the window.
        var vm = MakeSetSyncPointViewModel();
        var lines = TwoLines();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: LentWaveform());
        var window = Track(new SetSyncPointWindow(vm));
        Assert.True(window.MaxHeight < 400, $"cap was {window.MaxHeight}");

        vm.IsAudioVisualizerVisible = true;

        Assert.True(double.IsPositiveInfinity(window.MaxHeight));
        var split = Assert.Single(SplitsOf(window));
        Assert.False(HandleOf(split).IsVisible); // nothing above the waveform to trade height with
        Assert.True(split.RowDefinitions[2].Height.IsStar);
    }

    [AvaloniaFact]
    public void VisualSyncWindow_DragOnOnePane_MovesTheOtherAndIsRemembered()
    {
        Se.Settings.Synchronization.VisualSyncWaveformHeight = 80;
        var vm = MakeVisualSyncViewModel();
        vm.Initialize(TwoLines(), videoFileName: "/does/not/exist.mp4", subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: LentWaveform());
        var window = new VisualSyncWindow(vm);
        vm.IsAudioVisualizerVisible = true;

        var splits = SplitsOf(window);
        Assert.Equal(2, splits.Count);
        Assert.All(splits, s => Assert.True(HandleOf(s).IsVisible));

        SimulateDrag(splits[0], 130);
        Assert.Equal(130, splits[1].RowDefinitions[2].Height.Value);

        SimulateDrag(splits[1], 95);
        Assert.Equal(95, splits[0].RowDefinitions[2].Height.Value);

        window.Close();
        Assert.Equal(95, Se.Settings.Synchronization.VisualSyncWaveformHeight);
    }

    [AvaloniaFact]
    public void VisualSyncWindow_LosingTheWaveform_CollapsesBothRows()
    {
        // Opening a different video from the dialog drops the lent peaks (they belong to the
        // video the dialog was opened with), and the rows must go with them.
        var vm = MakeVisualSyncViewModel();
        vm.Initialize(TwoLines(), videoFileName: "/does/not/exist.mp4", subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: LentWaveform());
        var window = Track(new VisualSyncWindow(vm));
        vm.IsAudioVisualizerVisible = true;

        vm.IsAudioVisualizerVisible = false;

        Assert.All(SplitsOf(window), s =>
        {
            Assert.False(HandleOf(s).IsVisible);
            Assert.Equal(0, s.RowDefinitions[2].Height.Value);
        });
    }
}
