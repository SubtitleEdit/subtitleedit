using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.RemoveBurnedIn;

public class RemoveBurnedInWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [AvaloniaFact]
    public void Window_Constructs_And_Shows_Lines()
    {
        var vm = new RemoveBurnedInViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new FolderHelper());
        vm.Initialize("/videos/movie.mp4", 1920, 1080, 60, 0, 720, 1920, 360, 200, 5, new[]
        {
            new VideoOcrLineItem { StartTime = TimeSpan.FromSeconds(3), EndTime = TimeSpan.FromSeconds(5), Text = "Second" },
            new VideoOcrLineItem { StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2), Text = "First" },
            new VideoOcrLineItem { StartTime = TimeSpan.FromSeconds(6), EndTime = TimeSpan.FromSeconds(7), Text = " " },
        });
        var window = new RemoveBurnedInWindow(vm);
        _windows.Add(window);
        window.Show();
        window.UpdateLayout();

        Assert.NotNull(window.Content);
        Assert.Equal(2, vm.Lines.Count);
        Assert.Equal("First", vm.SelectedLine?.Text);
        Assert.Equal(200, vm.BrightnessMinimum);
        Assert.Equal(BurnedInMaskBuilder.DefaultGrow(1080), vm.Grow);
        Assert.EndsWith("movie.no-subs.mp4", vm.OutputFileName.Replace('\\', '/'));
        Assert.Equal(4, vm.Methods.Count);
        Assert.NotNull(vm.AudioVisualizer); // the timeline is built with the window
        Assert.Equal(1.5, vm.PositionSeconds, 3); // middle of the first (selected) line

        // Leaving a line out is reflected in the summary, and the next line moves the position.
        vm.Lines[0].IsIncluded = false;
        Assert.StartsWith("1 ", vm.LinesSummary);
        vm.NextLineCommand.Execute(null);
        Assert.Equal("Second", vm.SelectedLine?.Text);
        Assert.Equal(4.0, vm.PositionSeconds, 3);
    }

    [AvaloniaFact]
    public void Insert_And_Delete_Rectangle()
    {
        var vm = new RemoveBurnedInViewModel(new WindowService(new NullServiceProvider()), new FileHelper(), new FolderHelper());
        vm.Initialize("/videos/movie.mp4", 1920, 1080, 60, 0, 720, 1920, 360, 200, 5, new[]
        {
            new VideoOcrLineItem { StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2), Text = "First" },
            new VideoOcrLineItem { StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(12), Text = "Second" },
        });
        var window = new RemoveBurnedInWindow(vm);
        _windows.Add(window);
        window.Show();

        vm.PositionSeconds = 5;
        vm.InsertRectangleCommand.Execute(null);

        // Two seconds around the position, in time order, renumbered and selected.
        Assert.Equal(3, vm.Lines.Count);
        var rectangle = vm.Lines[1];
        Assert.True(rectangle.IsRectangle);
        Assert.Same(rectangle, vm.SelectedLine);
        Assert.True(vm.IsRectangleSelected);
        Assert.Equal(TimeSpan.FromSeconds(4), rectangle.Line.StartTime);
        Assert.Equal(TimeSpan.FromSeconds(6), rectangle.Line.EndTime);
        Assert.Equal(new[] { 1, 2, 3 }, vm.Lines.Select(l => l.Number));
        Assert.Equal(new[] { 480, 810, 960, 180 }, new[] { rectangle.RectX, rectangle.RectY, rectangle.RectWidth, rectangle.RectHeight });

        // Drawing on the preview (the selector's two-way bound fields) updates the line.
        vm.SelectedRectX = 1500;
        vm.SelectedRectWidth = 300;
        Assert.Equal(1500, rectangle.RectX);
        Assert.Equal("Rectangle 300x180", rectangle.Text);
        Assert.Equal(new BurnedInRectangle(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(6), 1500, 810, 300, 180), rectangle.ToRectangle());

        // Delete removes rectangles and OCR lines alike, and selects the next line.
        vm.SelectedLine = rectangle;
        vm.DeleteLineCommand.Execute(null);
        Assert.Equal(2, vm.Lines.Count);
        Assert.DoesNotContain(rectangle, vm.Lines);
        Assert.Equal("Second", vm.SelectedLine?.Text);
        vm.DeleteLines(new[] { vm.Lines[0] });
        Assert.Equal("Second", Assert.Single(vm.Lines).Text);
        Assert.Equal(1, vm.Lines[0].Number);

        // A timeline selection (drag on an empty spot + Enter) inserts a rectangle for that span.
        vm.AudioVisualizerNewSelectionInsert(vm.AudioVisualizer!, new Nikse.SubtitleEdit.Controls.AudioVisualizerControl.ParagraphEventArgs(
            new Nikse.SubtitleEdit.Features.Main.SubtitleLineViewModel { StartTime = TimeSpan.FromSeconds(20), EndTime = TimeSpan.FromSeconds(23.5) }));
        Assert.Equal(2, vm.Lines.Count);
        Assert.True(vm.SelectedLine?.IsRectangle);
        Assert.Equal(TimeSpan.FromSeconds(20), vm.SelectedLine!.Line.StartTime);
        Assert.Equal(TimeSpan.FromSeconds(23.5), vm.SelectedLine.Line.EndTime);
        Assert.Equal(2, vm.SelectedLine.Number);
    }
}
