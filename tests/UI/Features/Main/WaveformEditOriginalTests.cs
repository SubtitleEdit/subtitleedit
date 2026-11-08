using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// The waveform's A/B switch (#16354): with it on, the waveform drags the original's own time
/// codes and leaves the working subtitle alone; it only stays on with an editable original.
/// </summary>
public class WaveformEditOriginalTests
{
    private const int SampleRate = 126;

    [AvaloniaFact]
    public void Toggle_WithAnEditableOriginal_SwitchesTheWaveform()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            ImportOriginal(vm, Original(("Original one", 0, 2000)));

            vm.ToggleWaveformEditOriginalCommand.Execute(null);
            Settle(window);
            Assert.True(vm.IsWaveformEditingOriginal);
            Assert.True(vm.AudioVisualizer!.EditOriginalTimeCodes);

            vm.ToggleWaveformEditOriginalCommand.Execute(null);
            Settle(window);
            Assert.False(vm.IsWaveformEditingOriginal);
            Assert.False(vm.AudioVisualizer!.EditOriginalTimeCodes);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Toggle_WithAReadOnlyOriginal_SwitchesBack()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            ImportOriginal(vm, Original(("Original one", 0, 2000)), isReadOnly: true);

            vm.ToggleWaveformEditOriginalCommand.Execute(null);
            Settle(window);

            Assert.False(vm.IsWaveformEditingOriginal);
            Assert.False(vm.AudioVisualizer!.EditOriginalTimeCodes);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Toggle_WithoutAnOriginal_StaysOff()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);

            vm.ToggleWaveformEditOriginalCommand.Execute(null);
            Settle(window);

            Assert.False(vm.IsWaveformEditingOriginal);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void MakingTheOriginalReadOnly_SwitchesBack()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            ImportOriginal(vm, Original(("Original one", 0, 2000)));
            vm.IsWaveformEditingOriginal = true;
            Settle(window);
            Assert.True(vm.IsWaveformEditingOriginal);

            vm.IsOriginalReadOnly = true;
            Settle(window);

            Assert.False(vm.IsWaveformEditingOriginal);
            Assert.False(vm.AudioVisualizer!.EditOriginalTimeCodes);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Dragging_MovesTheOriginalAndLeavesTheTranslation()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            ImportOriginal(vm, Original(("Original one", 0, 2000), ("Original two", 4000, 6000)));
            var av = PrepareWaveform(window, vm);
            vm.IsWaveformEditingOriginal = true;
            Settle(window);

            var row = vm.Subtitles[0];
            Drag(window, av, fromSeconds: 1.0, toSeconds: 2.0);

            Assert.Equal(1000, row.EffectiveOriginalStartTime.TotalMilliseconds);
            Assert.Equal(3000, row.EffectiveOriginalEndTime.TotalMilliseconds);
            Assert.Equal(0, row.StartTime.TotalMilliseconds);
            Assert.Equal(2000, row.EndTime.TotalMilliseconds);
            Assert.Equal(4000, vm.Subtitles[1].EffectiveOriginalStartTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void DraggingAnEdge_ResizesTheOriginal()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            ImportOriginal(vm, Original(("Original one", 0, 2000)));
            var av = PrepareWaveform(window, vm);
            vm.IsWaveformEditingOriginal = true;
            Settle(window);

            var row = vm.Subtitles[0];
            Drag(window, av, fromSeconds: 2.0, toSeconds: 3.0);

            Assert.Equal(0, row.EffectiveOriginalStartTime.TotalMilliseconds);
            Assert.Equal(3000, row.EffectiveOriginalEndTime.TotalMilliseconds);
            Assert.Equal(2000, row.EndTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void DraggingOverOnlyATranslationBlock_ChangesNothing()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            ImportOriginal(vm, Original(("Original one", 8000, 9000)));
            var av = PrepareWaveform(window, vm);
            vm.IsWaveformEditingOriginal = true;
            Settle(window);

            var row = vm.Subtitles[0];
            var countBefore = vm.Subtitles.Count;
            Drag(window, av, fromSeconds: 1.0, toSeconds: 2.5);

            Assert.Equal(0, row.StartTime.TotalMilliseconds);
            Assert.Equal(2000, row.EndTime.TotalMilliseconds);
            Assert.Equal(8000, row.EffectiveOriginalStartTime.TotalMilliseconds);
            Assert.Equal(countBefore, vm.Subtitles.Count);
            Assert.Null(av.NewSelectionParagraph);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void SwitchedOff_DraggingMovesTheTranslation()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            ImportOriginal(vm, Original(("Original one", 0, 2000)));
            var av = PrepareWaveform(window, vm);

            var row = vm.Subtitles[0];
            Drag(window, av, fromSeconds: 2.0, toSeconds: 3.0);

            Assert.Equal(3000, row.EndTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static Nikse.SubtitleEdit.Controls.AudioVisualizerControl.AudioVisualizer PrepareWaveform(Window window, MainViewModel vm)
    {
        var av = vm.AudioVisualizer!;
        var peaks = new WavePeak2[SampleRate * 60];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        av.WavePeaks = new WavePeakData2(SampleRate, peaks);
        av.ZoomFactor = 1;
        av.SetPosition(0, vm.Subtitles, 0, -1, new List<SubtitleLineViewModel>());
        Settle(window);
        return av;
    }

    private static void Drag(Window window, Nikse.SubtitleEdit.Controls.AudioVisualizerControl.AudioVisualizer av, double fromSeconds, double toSeconds)
    {
        var origin = av.TranslatePoint(new Point(0, 0), window)!.Value;
        var y = origin.Y + av.Bounds.Height * 0.75;
        var from = new Point(origin.X + fromSeconds * SampleRate, y);
        var to = new Point(origin.X + toSeconds * SampleRate, y);
        window.MouseDown(from, MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point((from.X + to.X) / 2, y), RawInputModifiers.LeftMouseButton);
        window.MouseMove(to, RawInputModifiers.LeftMouseButton);
        window.MouseUp(to, MouseButton.Left, RawInputModifiers.None);
        Settle(window);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static Subtitle Original(params (string Text, int StartMs, int EndMs)[] lines)
    {
        var subtitle = new Subtitle();
        foreach (var (text, startMs, endMs) in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, startMs, endMs));
        }

        return subtitle;
    }

    private static void ImportOriginal(MainViewModel vm, Subtitle original, bool isReadOnly = false)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");
        method.Invoke(vm, new object?[] { 0, "original.srt", original, (ImportOriginalHelper.OriginalMatch?)null, isReadOnly });
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
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
