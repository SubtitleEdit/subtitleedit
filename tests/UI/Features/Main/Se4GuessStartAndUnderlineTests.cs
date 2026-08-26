using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using Xunit;

namespace UITests.Features.Main;

/// <summary>
/// SE 4 parity commands that had no SE 5 counterpart: "toggle underline" (the third list view
/// formatting toggle next to italic/bold) and the waveform's "guess start".
/// </summary>
public class Se4GuessStartAndUnderlineTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly bool _timeCodesLocked = Se.Settings.General.LockTimeCodes;

    public void Dispose()
    {
        Se.Settings.General.LockTimeCodes = _timeCodesLocked;
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
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
        return (window, vm);
    }

    private static void Select(MainViewModel vm, params SubtitleLineViewModel[] lines)
    {
        vm.SubtitleGrid.SelectedItems!.Clear();
        foreach (var line in lines)
        {
            vm.SubtitleGrid.SelectedItems!.Add(line);
        }

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void ToggleUnderlineWrapsAndUnwrapsSelectedLines()
    {
        var (_, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 1000, 3000), null!) { Number = 1 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("World", 4000, 6000), null!) { Number = 2 });
        Dispatcher.UIThread.RunJobs();

        Select(vm, vm.Subtitles[0], vm.Subtitles[1]);

        vm.ToggleLinesUnderlineOrSelectedTextCommand.Execute(null);
        Assert.Equal("<u>Hello</u>", vm.Subtitles[0].Text);
        Assert.Equal("<u>World</u>", vm.Subtitles[1].Text);

        // Second press removes it again - the first selected line decides for the whole selection.
        vm.ToggleLinesUnderlineOrSelectedTextCommand.Execute(null);
        Assert.Equal("Hello", vm.Subtitles[0].Text);
        Assert.Equal("World", vm.Subtitles[1].Text);
    }

    /// <summary>
    /// A cue that starts inside the silence in front of the speech: "guess start" moves the start
    /// cue up to just before the speech begins instead of leaving the dead air in the line.
    /// </summary>
    [AvaloniaFact]
    public void GuessStartMovesTheStartCueToJustBeforeTheSpeech()
    {
        Se.Settings.General.LockTimeCodes = false;

        var (window, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 2200, 3800), null!) { Number = 1 });
        Dispatcher.UIThread.RunJobs();

        var av = vm.AudioVisualizer!;
        av.WavePeaks = MakePeaks(sampleRate: 100, seconds: 6, speechFromSeconds: 2.5, speechToSeconds: 4.0);
        Select(vm, vm.Subtitles[0]);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        vm.WaveformGuessStartCommand.Execute(null);

        var line = vm.Subtitles[0];
        Assert.InRange(line.StartTime.TotalMilliseconds, 2400, 2500);
        Assert.Equal(3800, line.EndTime.TotalMilliseconds, 0);
    }

    [AvaloniaFact]
    public void GuessStartDoesNothingWithoutAWaveform()
    {
        Se.Settings.General.LockTimeCodes = false;

        var (_, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 2200, 3800), null!) { Number = 1 });
        Dispatcher.UIThread.RunJobs();
        Select(vm, vm.Subtitles[0]);

        vm.WaveformGuessStartCommand.Execute(null);

        Assert.Equal(2200, vm.Subtitles[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal(3800, vm.Subtitles[0].EndTime.TotalMilliseconds, 0);
    }

    private static WavePeakData2 MakePeaks(int sampleRate, int seconds, double speechFromSeconds, double speechToSeconds)
    {
        var peaks = new WavePeak2[sampleRate * seconds];
        var from = (int)(speechFromSeconds * sampleRate);
        var to = (int)(speechToSeconds * sampleRate);
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = i >= from && i < to ? new WavePeak2(8000, -8000) : new WavePeak2(0, 0);
        }

        return new WavePeakData2(sampleRate, peaks);
    }
}
