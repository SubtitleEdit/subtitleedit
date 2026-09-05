using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// The speech-to-text window builds its grid in code. Grid.Row values past the last
/// RowDefinition silently clamp onto the final row, which put the post-processing and
/// advanced-settings labels on top of the "Transcribing..." / "Time elapsed" text once
/// the Google Cloud engine added rows without extending the definitions.
/// </summary>
public class SpeechToTextWindowLayoutTests : IDisposable
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

    private (SpeechToTextWindow Window, Grid Grid) BuildWindow()
    {
        var vm = new SpeechToTextViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new FolderHelper());
        var window = new SpeechToTextWindow(vm);
        _windows.Add(window);
        return (window, Assert.IsType<Grid>(window.Content));
    }

    [AvaloniaFact]
    public void EveryChild_SitsInsideTheDefinedRows()
    {
        var (_, grid) = BuildWindow();

        var rowCount = grid.RowDefinitions.Count;
        foreach (var child in grid.Children)
        {
            var row = Grid.GetRow(child);
            var span = Grid.GetRowSpan(child);
            Assert.True(row + span <= rowCount, $"{child.GetType().Name} at row {row} span {span} exceeds {rowCount} rows");
        }
    }

    [AvaloniaFact]
    public void ProgressRow_HoldsOnlyTheProgressPanelAndButtons()
    {
        var (_, grid) = BuildWindow();

        var lastRow = grid.RowDefinitions.Count - 1;
        var onLastRow = grid.Children.Where(c => Grid.GetRow(c) == lastRow).ToList();

        Assert.Equal(2, onLastRow.Count);
        Assert.All(onLastRow, c => Assert.Equal(3, Grid.GetColumnSpan(c)));
    }

    [AvaloniaFact]
    public void ConsoleLog_SpansDownToTheRowAboveProgress()
    {
        var (_, grid) = BuildWindow();

        var lastRow = grid.RowDefinitions.Count - 1;
        var consoleViews = grid.Children.Where(c => Grid.GetRow(c) == 1 && Grid.GetColumn(c) == 2).ToList();

        Assert.NotEmpty(consoleViews);
        Assert.All(consoleViews, c => Assert.Equal(lastRow, Grid.GetRow(c) + Grid.GetRowSpan(c)));
    }
}
