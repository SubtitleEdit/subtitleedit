using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// The speech-to-text window builds its layout in code. Grid.Row values past the last
/// RowDefinition silently clamp onto the final row, which once put the post-processing and
/// advanced-settings labels on top of the "Transcribing..." / "Time elapsed" text when the
/// Google Cloud engine added rows without extending the definitions. The settings now live in
/// their own scrolling column, with the progress strip and the buttons in rows of their own.
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

    private (SpeechToTextWindow Window, SpeechToTextViewModel Vm, Grid Grid) BuildWindow()
    {
        var vm = new SpeechToTextViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new FolderHelper());
        var window = new SpeechToTextWindow(vm);
        _windows.Add(window);
        return (window, vm, Assert.IsType<Grid>(window.Content));
    }

    private static Control Named(Window window, string name) =>
        window.GetLogicalDescendants().OfType<Control>().Single(c => c.Name == name);

    [AvaloniaFact]
    public void EveryChild_SitsInsideTheDefinedRows()
    {
        var (window, _, _) = BuildWindow();

        foreach (var grid in window.GetLogicalDescendants().OfType<Grid>().Where(g => g.RowDefinitions.Count > 0))
        {
            var rowCount = grid.RowDefinitions.Count;
            foreach (var child in grid.Children)
            {
                var row = Grid.GetRow(child);
                var span = Grid.GetRowSpan(child);
                Assert.True(row + span <= rowCount, $"{child.GetType().Name} at row {row} span {span} exceeds {rowCount} rows");
            }
        }
    }

    [AvaloniaFact]
    public void ProgressAndButtons_HaveRowsOfTheirOwn()
    {
        var (window, _, grid) = BuildWindow();

        var progress = Named(window, SpeechToTextWindow.ProgressPanelName);
        var buttons = Named(window, SpeechToTextWindow.ButtonBarName);
        Assert.Same(grid, progress.Parent);
        Assert.Same(grid, buttons.Parent);

        var progressRow = Grid.GetRow(progress);
        var buttonRow = Grid.GetRow(buttons);
        Assert.NotEqual(progressRow, buttonRow);
        foreach (var row in new[] { progressRow, buttonRow })
        {
            Assert.Single(grid.Children, c => Grid.GetRow(c) == row);
            Assert.True(grid.RowDefinitions[row].Height.IsAuto);
        }
    }

    [AvaloniaFact]
    public void SettingsAndConsoleLog_ShareTheStretchingRowAboveProgress()
    {
        var (window, vm, grid) = BuildWindow();

        var progressRow = Grid.GetRow(Named(window, SpeechToTextWindow.ProgressPanelName));
        var content = grid.Children.Single(c => Grid.GetRow(c) < progressRow);
        Assert.True(grid.RowDefinitions[Grid.GetRow(content)].Height.IsStar);

        // The settings column scrolls, so no settings row can spill onto the progress text
        var settings = Named(window, SpeechToTextWindow.SettingsPanelName);
        Assert.Contains(settings, content.GetLogicalDescendants());
        Assert.IsType<ScrollViewer>(((Border)settings).Child);

        Assert.Contains(vm.TextBoxConsoleLogSingle, content.GetLogicalDescendants());
        Assert.Contains(vm.TextBoxConsoleLogBatch, content.GetLogicalDescendants());
    }
}
