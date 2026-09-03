using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly int _guessStartOffsetMs = Se.Settings.Waveform.GuessStartOffsetMs;
    private readonly int _guessEndOffsetMs = Se.Settings.Waveform.GuessEndOffsetMs;

    public void Dispose()
    {
        Se.Settings.General.LockTimeCodes = _timeCodesLocked;
        Se.Settings.Waveform.GuessStartOffsetMs = _guessStartOffsetMs;
        Se.Settings.Waveform.GuessEndOffsetMs = _guessEndOffsetMs;
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

    /// <summary>
    /// #14472: the guessed start "feels too close to the waveform" for some users - the offset
    /// setting pads the detected boundary by moving the start earlier.
    /// </summary>
    [AvaloniaFact]
    public void GuessStartHonorsTheOffsetSetting()
    {
        Se.Settings.General.LockTimeCodes = false;
        Se.Settings.Waveform.GuessStartOffsetMs = 100;

        var (window, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 2200, 3800), null!) { Number = 1 });
        Dispatcher.UIThread.RunJobs();

        var av = vm.AudioVisualizer!;
        av.WavePeaks = MakePeaks(sampleRate: 100, seconds: 6, speechFromSeconds: 2.5, speechToSeconds: 4.0);
        Select(vm, vm.Subtitles[0]);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        vm.WaveformGuessStartCommand.Execute(null);

        Assert.InRange(vm.Subtitles[0].StartTime.TotalMilliseconds, 2300, 2400);
    }

    /// <summary>
    /// "Guess end" (#14472): a cue that ends inside the silence after the speech is pulled back to
    /// just after the speech stops.
    /// </summary>
    [AvaloniaFact]
    public void GuessEndMovesTheEndCueToJustAfterTheSpeech()
    {
        Se.Settings.General.LockTimeCodes = false;
        Se.Settings.Waveform.GuessEndOffsetMs = 0;

        var (window, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 2500, 4600), null!) { Number = 1 });
        Dispatcher.UIThread.RunJobs();

        var av = vm.AudioVisualizer!;
        av.WavePeaks = MakePeaks(sampleRate: 100, seconds: 7, speechFromSeconds: 2.5, speechToSeconds: 4.0);
        Select(vm, vm.Subtitles[0]);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        vm.WaveformGuessEndCommand.Execute(null);

        var line = vm.Subtitles[0];
        Assert.Equal(2500, line.StartTime.TotalMilliseconds, 0);
        Assert.InRange(line.EndTime.TotalMilliseconds, 4000, 4100);
    }

    /// <summary>
    /// A cue that ends while the speech is still going is extended to the silence after it.
    /// </summary>
    [AvaloniaFact]
    public void GuessEndExtendsAnEndCueThatCutsTheSpeechShort()
    {
        Se.Settings.General.LockTimeCodes = false;
        Se.Settings.Waveform.GuessEndOffsetMs = 0;

        var (window, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 2500, 3500), null!) { Number = 1 });
        Dispatcher.UIThread.RunJobs();

        var av = vm.AudioVisualizer!;
        av.WavePeaks = MakePeaks(sampleRate: 100, seconds: 7, speechFromSeconds: 2.5, speechToSeconds: 4.0);
        Select(vm, vm.Subtitles[0]);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        vm.WaveformGuessEndCommand.Execute(null);

        Assert.InRange(vm.Subtitles[0].EndTime.TotalMilliseconds, 4000, 4100);
    }

    /// <summary>
    /// The end never runs into the next line: it stops the minimum gap before it.
    /// </summary>
    [AvaloniaFact]
    public void GuessEndStopsAtTheNextLine()
    {
        Se.Settings.General.LockTimeCodes = false;
        Se.Settings.Waveform.GuessEndOffsetMs = 0;

        var (window, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 2500, 3500), null!) { Number = 1 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("World", 3800, 5000), null!) { Number = 2 });
        Dispatcher.UIThread.RunJobs();

        var av = vm.AudioVisualizer!;
        av.WavePeaks = MakePeaks(sampleRate: 100, seconds: 7, speechFromSeconds: 2.5, speechToSeconds: 4.0);
        Select(vm, vm.Subtitles[0]);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        vm.WaveformGuessEndCommand.Execute(null);

        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        Assert.Equal(3800 - gapMs, vm.Subtitles[0].EndTime.TotalMilliseconds, 0);
    }

    /// <summary>
    /// #14472: as Waveform-category shortcuts "guess start/end" only fired while the waveform had
    /// keyboard focus, which it rarely has - they must dispatch from anywhere like in SE 4.
    /// </summary>
    [AvaloniaFact]
    public void GuessStartAndEndAreGeneralShortcuts()
    {
        var (_, vm) = CreateMainViewModel();

        var all = ShortcutsMain.GetAllShortcuts(vm);
        var start = all.Single(s => s.Name == nameof(MainViewModel.WaveformGuessStartCommand));
        var end = all.Single(s => s.Name == nameof(MainViewModel.WaveformGuessEndCommand));

        Assert.Equal(ShortcutCategory.General, start.Category);
        Assert.Equal(ShortcutCategory.General, end.Category);
        Assert.Equal(ShortcutGroup.Waveform, start.Group);
        Assert.Equal(ShortcutGroup.Waveform, end.Group);
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
