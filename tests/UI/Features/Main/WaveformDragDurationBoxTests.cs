using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// Dragging a cue's end edge on the waveform must tick the edit box's Duration up/down live -
/// with ShowUpDownEndTime off (the reporter's setup) the duration box is the only live readout
/// of an end drag.
/// </summary>
public class WaveformDragDurationBoxTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly bool _showEnd = Se.Settings.Appearance.ShowUpDownEndTime;
    private readonly bool _showDuration = Se.Settings.Appearance.ShowUpDownDuration;
    private readonly bool _frameMode = Se.Settings.General.UseFrameMode;
    private readonly bool _snapFrames = Se.Settings.Waveform.SnapToFrames;
    private readonly bool _snapCuts = Se.Settings.Waveform.SnapToShotChanges;
    private readonly int _layout = Se.Settings.General.LayoutNumber;

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        Se.Settings.Appearance.ShowUpDownEndTime = _showEnd;
        Se.Settings.Appearance.ShowUpDownDuration = _showDuration;
        Se.Settings.General.UseFrameMode = _frameMode;
        Se.Settings.Waveform.SnapToFrames = _snapFrames;
        Se.Settings.Waveform.SnapToShotChanges = _snapCuts;
        Se.Settings.General.LayoutNumber = _layout;
    }

    private static WavePeakData2 MakePeaks(int sampleRate, int seconds)
    {
        var peaks = new WavePeak2[sampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        return new WavePeakData2(sampleRate, peaks);
    }

    [AvaloniaFact]
    public void DraggingTheEndEdgeTicksTheDurationBox()
    {
        // The reporter's setup: layout 1, ms mode, Start + Duration up/downs but no End box.
        Se.Settings.General.LayoutNumber = 1;
        Se.Settings.General.UseFrameMode = false;
        Se.Settings.Appearance.ShowUpDownEndTime = false;
        Se.Settings.Appearance.ShowUpDownDuration = true;
        Se.Settings.Waveform.SnapToFrames = false;
        Se.Settings.Waveform.SnapToShotChanges = true; // on, like the reporter; no shot-change list loaded

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

        for (var i = 0; i < 3; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 5000 + 1000, i * 5000 + 3000), null!)
            {
                Number = i + 1,
            });
        }

        vm.SelectedSubtitle = vm.Subtitles[0];
        var av = vm.AudioVisualizer;
        av.WavePeaks = MakePeaks(126, 60);
        av.SetPosition(0, vm.Subtitles, 0, 0, new List<SubtitleLineViewModel> { vm.Subtitles[0] });
        Settle(window);

        var line = vm.Subtitles[0]; // 1000..3000 ms
        Assert.Same(line, av.SelectedParagraph);

        var durationBox = window.GetVisualDescendants().OfType<SecondsUpDown>()
            .Single(c => c.IsEffectivelyVisible);
        var durationText = durationBox.GetVisualDescendants().OfType<TextBox>().Single();
        Assert.Equal("2.000", durationText.Text!.Replace(',', '.'));

        // The waveform's own coordinate frame: end edge of 3 s at zoom 1 with 126 px/s.
        var origin = av.TranslatePoint(new Point(0, 0), window)!.Value;
        var y = origin.Y + av.Bounds.Height / 2;
        var endEdgeX = origin.X + 3.0 * 126;

        window.MouseDown(new Point(endEdgeX, y), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(endEdgeX + 126, y), RawInputModifiers.None);
        Settle(window);

        // Mid-drag: the line's end moved a second; the duration box must show it.
        Assert.Equal(4000, line.EndTime.TotalMilliseconds);
        Assert.Equal(3000, line.Duration.TotalMilliseconds);
        Assert.Equal("3.000", durationText.Text!.Replace(',', '.'));

        window.MouseUp(new Point(endEdgeX + 126, y), MouseButton.Left, RawInputModifiers.None);
        Settle(window);
        Assert.Equal("3.000", durationText.Text!.Replace(',', '.'));
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
