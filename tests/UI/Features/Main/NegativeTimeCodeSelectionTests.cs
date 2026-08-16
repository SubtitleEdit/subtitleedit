using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// Cutting a "previously on..." off the front of a video means adjusting all times by a negative
/// offset, which legitimately moves the first lines before zero. Selecting such a line then wrote a
/// zero straight back into it through the start/end editors' two-way binding, so walking the list
/// zeroed one time code after another (#13695).
/// </summary>
public class NegativeTimeCodeSelectionTests : IDisposable
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

    private (Window Window, MainViewModel Vm) ShowMainWindowWithLines(int lineCount)
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

        for (var i = 0; i < lineCount; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        Settle(window);
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    [AvaloniaFact]
    public void AdjustAllTimesBeforeZero_SurvivesSelectingEveryLine()
    {
        var (window, vm) = ShowMainWindowWithLines(4);

        // "Show earlier" by 3 seconds - the first two lines end up before zero.
        vm.Adjust(TimeSpan.FromSeconds(-3), adjustAll: true, adjustSelectedLines: false, adjustSelectedLinesAndForward: false);
        Settle(window);

        var expected = vm.Subtitles
            .Select(p => (Start: p.StartTime, End: p.EndTime))
            .ToList();
        Assert.Equal(-3000, expected[0].Start.TotalMilliseconds);
        Assert.Equal(-1000, expected[1].Start.TotalMilliseconds);

        foreach (var line in vm.Subtitles.ToList())
        {
            vm.SelectAndScrollToSubtitle(line);
            Settle(window);
        }

        for (var i = 0; i < vm.Subtitles.Count; i++)
        {
            Assert.Equal(expected[i].Start, vm.Subtitles[i].StartTime);
            Assert.Equal(expected[i].End, vm.Subtitles[i].EndTime);
        }
    }

    [AvaloniaFact]
    public void SelectedNegativeLine_IsShownWithItsSignInTheStartTimeEditor()
    {
        var (window, vm) = ShowMainWindowWithLines(2);

        vm.Adjust(TimeSpan.FromSeconds(-3), adjustAll: true, adjustSelectedLines: false, adjustSelectedLinesAndForward: false);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[0]);
        Settle(window);

        var startTimeEditor = window.GetVisualDescendants()
            .OfType<TimeCodeUpDown>()
            .First(c => (string?)AutomationProperties.GetName(c) == Se.Language.General.StartTime);
        var textBox = startTimeEditor.GetVisualDescendants().OfType<TextBox>().Single();

        Assert.Equal(TimeSpan.FromSeconds(-3), startTimeEditor.Value);
        Assert.StartsWith("-00:00:03", textBox.Text);
    }
}
