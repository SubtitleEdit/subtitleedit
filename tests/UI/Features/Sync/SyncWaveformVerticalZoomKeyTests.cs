using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Sync;

/// <summary>
/// Shift + plus/minus zooms the sync dialogs' waveforms vertically (#14487). The first cut only
/// matched the numeric-keypad keys (Key.Add / Key.Subtract), so a laptop or a Spanish keyboard,
/// whose plus and minus keys report as OemPlus / OemMinus, could not zoom at all (#14419 comment).
/// </summary>
public class SyncWaveformVerticalZoomKeyTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
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

    private static KeyEventArgs ShiftKey(Key key)
        => new() { RoutedEvent = InputElement.KeyDownEvent, Key = key, KeyModifiers = KeyModifiers.Shift };

    private static SetSyncPointViewModel MakeSetSyncPointViewModel()
    {
        var vm = new SetSyncPointViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));
        var lines = TwoLines();
        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: LentWaveform());
        vm.IsAudioVisualizerVisible = true; // handed over on a posted job that these tests never pump
        return vm;
    }

    [AvaloniaTheory]
    [InlineData(Key.Add, 0.9)]
    [InlineData(Key.OemPlus, 0.9)]
    [InlineData(Key.Subtract, 1.1)]
    [InlineData(Key.OemMinus, 1.1)]
    public void SetSyncPoint_ShiftPlusOrMinus_ZoomsFromKeypadAndMainRow(Key key, double expectedZoom)
    {
        var vm = MakeSetSyncPointViewModel();
        var e = ShiftKey(key);

        vm.OnKeyDownHandler(null, e);

        Assert.True(e.Handled);
        Assert.Equal(expectedZoom, vm.AudioVisualizer.VerticalZoomFactor, 6);
    }

    [AvaloniaFact]
    public void SetSyncPoint_PlusWithoutShift_DoesNotZoom()
    {
        var vm = MakeSetSyncPointViewModel();
        var e = new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.OemPlus, KeyModifiers = KeyModifiers.None };

        vm.OnKeyDownHandler(null, e);

        Assert.False(e.Handled);
        Assert.Equal(1.0, vm.AudioVisualizer.VerticalZoomFactor, 6);
    }

    private VisualSyncViewModel ShowVisualSync()
    {
        var vm = new VisualSyncViewModel(new WindowService(new NullServiceProvider()), new FileHelper(),
            new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()),
            new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));
        vm.Initialize(TwoLines(), videoFileName: null, subtitleFileName: null, previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: LentWaveform());
        var window = new VisualSyncWindow(vm);
        _windows.Add(window);
        vm.IsAudioVisualizerVisible = true;
        window.Show();
        return vm;
    }

    [AvaloniaTheory]
    [InlineData(Key.OemPlus, 0.9)]
    [InlineData(Key.OemMinus, 1.1)]
    public void VisualSync_ShiftMainRowPlusOrMinus_ZoomsTheFocusedPane(Key key, double expectedZoom)
    {
        var vm = ShowVisualSync();
        vm.AudioVisualizerRight.Focus();
        Assert.True(vm.AudioVisualizerRight.IsFocused);
        var e = ShiftKey(key);

        vm.OnKeyDownHandler(null, e);

        Assert.True(e.Handled);
        Assert.Equal(expectedZoom, vm.AudioVisualizerRight.VerticalZoomFactor, 6);
        Assert.Equal(1.0, vm.AudioVisualizerLeft.VerticalZoomFactor, 6); // only the focused pane zooms
    }

    // Focus normally sits on a child of a pane (a button under the player, the player's position
    // slider), never on the player itself - keys used to be dropped then (#15789).
    [AvaloniaFact]
    public void VisualSync_FocusOnButtonInsidePane_KeysGoToThatPane()
    {
        var vm = ShowVisualSync();
        var button = vm.PaneRight!.GetVisualDescendants().OfType<Button>().First(b => b.IsEffectivelyVisible);
        Assert.True(button.Focus());
        var e = ShiftKey(Key.OemPlus);

        vm.OnKeyDownHandler(null, e);

        Assert.True(e.Handled);
        Assert.Equal(0.9, vm.AudioVisualizerRight.VerticalZoomFactor, 6);
        Assert.Equal(1.0, vm.AudioVisualizerLeft.VerticalZoomFactor, 6);
    }

    private static Button OkButton(VisualSyncViewModel vm)
        => vm.Window!.GetVisualDescendants().OfType<Button>().First(b => b.Command == vm.OkCommand);

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void VisualSync_FocusOutsidePanes_KeysGoToLastActivePane(bool rightActive)
    {
        var vm = ShowVisualSync();
        vm.IsRightPaneActive = rightActive;
        Assert.True(OkButton(vm).Focus()); // in the Sync/OK/Cancel bar, outside both panes
        var e = ShiftKey(Key.OemPlus);

        vm.OnKeyDownHandler(null, e);

        Assert.True(e.Handled);
        Assert.Equal(rightActive ? 0.9 : 1.0, vm.AudioVisualizerRight.VerticalZoomFactor, 6);
        Assert.Equal(rightActive ? 1.0 : 0.9, vm.AudioVisualizerLeft.VerticalZoomFactor, 6);
    }

    [AvaloniaFact]
    public void VisualSync_SpaceOnButtonOutsidePanes_IsLeftToTheButton()
    {
        var vm = ShowVisualSync();
        var okButton = OkButton(vm);
        var e = new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Space, KeyModifiers = KeyModifiers.None, Source = okButton };

        vm.OnKeyDownHandler(null, e);

        Assert.False(e.Handled);
    }
}
