using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Controls;

// Repro for "when moving end in the waveform, the duration box time is not changing":
// dragging a cue edge mutates the shared SubtitleLineViewModel, and a SecondsUpDown bound
// TwoWay to its Duration must tick live during the drag.
public partial class AudioVisualizerDragDurationBoxTests
{
    private const int SampleRate = 126;
    private const double WidthPx = 800;
    private const double HeightPx = 200;

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    public partial class Vm : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private SubtitleLineViewModel? _selectedSubtitle;
    }

    // Same, but through the app's actual binding shape: DataContext = main view model,
    // path "SelectedSubtitle.Duration" (see InitListViewAndEditBox).
    [AvaloniaFact]
    public void DraggingTheEndEdgeUpdatesTheDurationBoxBoundViaSelectedSubtitle()
    {
        var snapToFrames = Se.Settings.Waveform.SnapToFrames;
        var snapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;
        var frameMode = Se.Settings.General.UseFrameMode;
        Se.Settings.Waveform.SnapToFrames = false;
        Se.Settings.Waveform.SnapToShotChanges = false;
        Se.Settings.General.UseFrameMode = false;
        try
        {
            var line = new SubtitleLineViewModel
            {
                Text = "text",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            };
            var vm = new Vm { SelectedSubtitle = line };

            var av = new AudioVisualizer
            {
                WavePeaks = MakePeaks(60),
                Width = WidthPx,
                Height = HeightPx,
            };
            av.SetPosition(0, new List<SubtitleLineViewModel> { line }, 0, 0, new List<SubtitleLineViewModel>());

            var durationBox = new SecondsUpDown
            {
                DataContext = vm,
                [!SecondsUpDown.ValueProperty] = new Binding($"{nameof(Vm.SelectedSubtitle)}.{nameof(SubtitleLineViewModel.Duration)}")
                {
                    Mode = BindingMode.TwoWay,
                },
            };

            var panel = new StackPanel();
            panel.Children.Add(av);
            panel.Children.Add(durationBox);
            var window = new Window { Width = WidthPx, Height = HeightPx + 40, Content = panel };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var textBox = durationBox.GetVisualDescendants().OfType<TextBox>().Single();
            Assert.Equal("2.000", textBox.Text!.Replace(',', '.'));

            window.MouseDown(new Point(378, 100), MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(new Point(378 + 126, 100), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(4000, line.EndTime.TotalMilliseconds);
            Assert.Equal(3000, line.Duration.TotalMilliseconds);
            Assert.Equal("3.000", textBox.Text!.Replace(',', '.'));

            window.MouseUp(new Point(378 + 126, 100), MouseButton.Left, RawInputModifiers.None);
        }
        finally
        {
            Se.Settings.Waveform.SnapToFrames = snapToFrames;
            Se.Settings.Waveform.SnapToShotChanges = snapToShotChanges;
            Se.Settings.General.UseFrameMode = frameMode;
        }
    }

    [AvaloniaFact]
    public void DraggingTheEndEdgeUpdatesABoundDurationBox()
    {
        var snapToFrames = Se.Settings.Waveform.SnapToFrames;
        var snapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;
        var frameMode = Se.Settings.General.UseFrameMode;
        Se.Settings.Waveform.SnapToFrames = false;
        Se.Settings.Waveform.SnapToShotChanges = false;
        Se.Settings.General.UseFrameMode = false;
        try
        {
            var line = new SubtitleLineViewModel
            {
                Text = "text",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            };

            var av = new AudioVisualizer
            {
                WavePeaks = MakePeaks(60),
                Width = WidthPx,
                Height = HeightPx,
            };
            av.SetPosition(0, new List<SubtitleLineViewModel> { line }, 0, 0, new List<SubtitleLineViewModel>());

            var durationBox = new SecondsUpDown { DataContext = line };
            durationBox[!SecondsUpDown.ValueProperty] =
                new Binding(nameof(SubtitleLineViewModel.Duration)) { Mode = BindingMode.TwoWay };

            var panel = new StackPanel();
            panel.Children.Add(av);
            panel.Children.Add(durationBox);
            var window = new Window { Width = WidthPx, Height = HeightPx + 40, Content = panel };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var textBox = durationBox.GetVisualDescendants().OfType<TextBox>().Single();
            Assert.Equal("2.000", textBox.Text!.Replace(',', '.'));

            // End edge is at 3 s = 378 px; grab it and drag +126 px (one second).
            window.MouseDown(new Point(378, 100), MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(new Point(378 + 126, 100), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            // Mid-drag, before pointer release: the box must already show the new duration.
            Assert.Equal(4000, line.EndTime.TotalMilliseconds);
            Assert.Equal(3000, line.Duration.TotalMilliseconds);
            Assert.Equal("3.000", textBox.Text!.Replace(',', '.'));

            window.MouseUp(new Point(378 + 126, 100), MouseButton.Left, RawInputModifiers.None);
        }
        finally
        {
            Se.Settings.Waveform.SnapToFrames = snapToFrames;
            Se.Settings.Waveform.SnapToShotChanges = snapToShotChanges;
            Se.Settings.General.UseFrameMode = frameMode;
        }
    }
}
