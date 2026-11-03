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
}
