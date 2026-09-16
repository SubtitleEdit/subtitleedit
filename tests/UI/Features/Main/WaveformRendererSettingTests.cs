using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using UITests;
using Xunit;

namespace Tests.Features.Main;

/// <summary>
/// "Use experimental fast renderer" picks between <see cref="AudioVisualizer"/> and
/// <see cref="SkiaAudioVisualizer"/>, which is a property of the control's type - so applying the
/// setting has to build a new control, where every other waveform setting is just assigned to the
/// existing one. The loaded audio has to come along, or the waveform would sit empty until the
/// user reopened the video.
/// </summary>
public class WaveformRendererSettingTests : IDisposable
{
    private readonly bool _originalUseSkiaRenderer = Se.Settings.Waveform.UseSkiaRenderer;

    public void Dispose() => Se.Settings.Waveform.UseSkiaRenderer = _originalUseSkiaRenderer;

    [AvaloniaFact]
    public void TogglingTheSettingSwapsTheRendererAndKeepsTheLoadedWaveform()
    {
        Se.Settings.Waveform.UseSkiaRenderer = false;

        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            var classic = vm.AudioVisualizer!;
            Assert.IsNotType<SkiaAudioVisualizer>(classic);

            // Stand in for an opened video: peaks, a zoom level and shot changes.
            var peaks = MakePeaks();
            var shotChanges = new List<double> { 1.5, 4.5 };
            classic.WavePeaks = peaks;
            classic.ShotChanges = shotChanges;
            classic.ZoomFactor = 2.5;
            classic.VerticalZoomFactor = 1.5;
            classic.StartPositionSeconds = 3;

            // What Settings > OK does after writing the setting.
            Se.Settings.Waveform.UseSkiaRenderer = true;
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            var skia = Assert.IsType<SkiaAudioVisualizer>(vm.AudioVisualizer);
            Assert.NotSame(classic, skia);
            Assert.Same(peaks, skia.WavePeaks);
            Assert.Same(shotChanges, skia.ShotChanges);
            Assert.Equal(2.5, skia.ZoomFactor);
            Assert.Equal(1.5, skia.VerticalZoomFactor);
            Assert.Equal(3, skia.StartPositionSeconds);

            // The replaced control must not be left in the tree drawing over the new one.
            Assert.Null(classic.Parent);

            // ...and back again.
            Se.Settings.Waveform.UseSkiaRenderer = false;
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            Assert.IsNotType<SkiaAudioVisualizer>(vm.AudioVisualizer);
            Assert.Same(peaks, vm.AudioVisualizer!.WavePeaks);
            Assert.Null(skia.Parent);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void TogglingTheSettingKeepsTheClickToGenerateHint()
    {
        // A video is open but no waveform has been generated yet: the control shows "click to
        // generate" and a click starts the generation. Both are per-control state, so swapping
        // the renderer must carry them over - or the hint and the click vanish until the video
        // is reopened.
        Se.Settings.Waveform.UseSkiaRenderer = false;

        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            var classic = vm.AudioVisualizer!;
            classic.WavePeaks = null;
            classic.ShowClickToGenerateHint = true;
            classic.ClickToGenerateText = "Click to generate waveform";

            Se.Settings.Waveform.UseSkiaRenderer = true;
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            var skia = Assert.IsType<SkiaAudioVisualizer>(vm.AudioVisualizer);
            Assert.NotSame(classic, skia);
            Assert.Null(skia.WavePeaks);
            Assert.True(skia.ShowClickToGenerateHint);
            Assert.Equal("Click to generate waveform", skia.ClickToGenerateText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void LayoutRebuildWithAnUnchangedSettingKeepsTheSameControl()
    {
        Se.Settings.Waveform.UseSkiaRenderer = true;

        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();
            var first = Assert.IsType<SkiaAudioVisualizer>(vm.AudioVisualizer);

            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            Assert.Same(first, vm.AudioVisualizer);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static WavePeakData2 MakePeaks()
    {
        var peaks = new WavePeak2[126 * 30];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(500, -500);
        }

        return new WavePeakData2(126, peaks);
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

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.SuppressSaveChangesPromptOnClose(vm);
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
