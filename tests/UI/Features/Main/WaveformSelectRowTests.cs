using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;
using UITests.Features.Sync.PointSync;

namespace UITests.Features.Main;

/// <summary>
/// A waveform event carries a copy of the hit line, so the row has to be found again in the grid
/// (#16500): a duplicate needs an id of its own, and lines that share a start time - ASSA layers -
/// must not resolve to the first of them.
/// </summary>
public class WaveformSelectRowTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _doubleClickAction = Se.Settings.Waveform.DoubleClickAction;
    private MainViewModel? _vm;

    public void Dispose()
    {
        Se.Settings.Waveform.DoubleClickAction = _doubleClickAction;
        if (_vm != null)
        {
            _vm.VideoPlayerControl = null;
        }

        foreach (var w in _windows)
        {
            w.Close();
        }
    }

    private MainViewModel MakeViewModel(params Paragraph[] paragraphs)
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
        _vm = vm;

        foreach (var p in paragraphs)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(p, null!) { Number = vm.Subtitles.Count + 1 });
        }

        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    private static void AddVideo(MainViewModel vm)
    {
        vm.VideoPlayerControl = new VideoPlayerControl(new WorkingVideoPlayer());
        typeof(MainViewModel).GetField("_videoFileName", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(vm, "video.mkv");
    }

    private static void Select(MainViewModel vm, SubtitleLineViewModel line)
    {
        vm.SelectedSubtitle = line;
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Duplicate_GetsItsOwnId()
    {
        var vm = MakeViewModel(new Paragraph("source", 1000, 3000));
        var source = vm.Subtitles[0];
        Select(vm, source);

        vm.DuplicateSelectedLinesCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal(new[] { "source", "source" }, vm.Subtitles.Select(p => p.Text));
        Assert.NotEqual(vm.Subtitles[0].Id, vm.Subtitles[1].Id);
    }

    [AvaloniaFact]
    public void SelectRequested_OnRetimedDuplicate_SelectsTheDuplicate()
    {
        var vm = MakeViewModel(new Paragraph("source", 1000, 3000));
        Select(vm, vm.Subtitles[0]);
        vm.DuplicateSelectedLinesCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        var duplicate = (SubtitleLineViewModel)vm.SubtitleGrid.SelectedItem!;
        var source = vm.Subtitles.Single(p => !ReferenceEquals(p, duplicate));
        duplicate.SetStartTimeKeepDuration(TimeSpan.FromSeconds(3));
        Select(vm, source);

        vm.AudioVisualizerSelectRequested(this, new ParagraphEventArgs(3.5, duplicate));
        Dispatcher.UIThread.RunJobs();

        Assert.Same(duplicate, vm.SubtitleGrid.SelectedItem);
    }

    [AvaloniaFact]
    public void ToggleSelection_OnSameStartLine_TogglesTheHitLine()
    {
        var vm = MakeViewModel(new Paragraph("bottom", 1000, 3000), new Paragraph("top", 1000, 3000), new Paragraph("next", 4000, 5000));
        AddVideo(vm);
        var next = vm.Subtitles[2];
        var top = vm.Subtitles[1];
        Select(vm, next);

        vm.AudioVisualizerOnToggleSelection(this, new ParagraphEventArgs(1.5, top));
        Dispatcher.UIThread.RunJobs();

        var selected = vm.SubtitleGrid.SelectedItems!.Cast<SubtitleLineViewModel>().ToList();
        Assert.Contains(top, selected);
        Assert.DoesNotContain(vm.Subtitles[0], selected);
    }

    [AvaloniaFact]
    public void DoubleClickSelect_OnSameStartLine_SelectsTheHitLine()
    {
        Se.Settings.Waveform.DoubleClickAction = "SelectSubtitle";
        var vm = MakeViewModel(new Paragraph("bottom", 1000, 3000), new Paragraph("top", 1000, 3000), new Paragraph("next", 4000, 5000));
        AddVideo(vm);
        var top = vm.Subtitles[1];
        Select(vm, vm.Subtitles[2]);

        vm.AudioVisualizerOnPrimaryDoubleClicked(this, new ParagraphNullableEventArgs(1.5, top));
        Dispatcher.UIThread.RunJobs();

        Assert.Same(top, vm.SubtitleGrid.SelectedItem);
    }
}
