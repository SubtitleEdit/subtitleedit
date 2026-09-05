using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.CutVideo;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;

namespace UITests.Features.Video.CutVideo;

/// <summary>
/// Cut video's waveform zoom follows the main window's configurable shortcut, whose default is
/// the numeric-keypad Shift+Add / Shift+Subtract. The main-row plus and minus keys (OemPlus /
/// OemMinus - all a laptop or a Spanish keyboard has) zoom as a fallback, like the sync dialogs
/// (#14419 comment).
/// </summary>
public class CutVideoVerticalZoomKeyTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static CutVideoViewModel MakeViewModel()
        => new(new FolderHelper(), new FileHelper(), new WindowService(new NullServiceProvider()), new InsertService(), new ShortcutManager());

    private static KeyEventArgs KeyArgs(Key key, KeyModifiers modifiers)
        => new() { RoutedEvent = InputElement.KeyDownEvent, Key = key, KeyModifiers = modifiers };

    [AvaloniaTheory]
    [InlineData(Avalonia.Input.Key.OemPlus, 0.9)]
    [InlineData(Avalonia.Input.Key.OemMinus, 1.1)]
    public void ShiftMainRowPlusOrMinus_ZoomsTheWaveform(Key key, double expectedZoom)
    {
        var vm = MakeViewModel();
        var e = KeyArgs(key, KeyModifiers.Shift);

        vm.OnKeyDownHandler(null, e);

        Assert.True(e.Handled);
        Assert.Equal(expectedZoom, vm.AudioVisualizer!.VerticalZoomFactor, 6);
    }

    [AvaloniaFact]
    public void MainRowPlusWithoutShift_DoesNotZoom()
    {
        var vm = MakeViewModel();
        var e = KeyArgs(Avalonia.Input.Key.OemPlus, KeyModifiers.None);

        vm.OnKeyDownHandler(null, e);

        Assert.False(e.Handled);
        Assert.Equal(1.0, vm.AudioVisualizer!.VerticalZoomFactor, 6);
    }
}
