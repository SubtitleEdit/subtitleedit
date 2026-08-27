using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.ApplyMinGap;

namespace UITests.Features.Tools.ApplyMinGap;

public class ApplyMinGapViewModelTests : IDisposable
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

    private static List<SubtitleLineViewModel> MakeSubtitles() => new()
    {
        new SubtitleLineViewModel { Text = "One", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) },
        new SubtitleLineViewModel { Text = "Two", StartTime = TimeSpan.FromSeconds(2), EndTime = TimeSpan.FromSeconds(3) },
        new SubtitleLineViewModel { Text = "Three", StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(6) },
    };

    private ApplyMinGapViewModel ShowWindow()
    {
        var vm = new ApplyMinGapViewModel();
        var window = new ApplyMinGapWindow(vm);
        _windows.Add(window);
        window.Show();
        vm.Initialize(MakeSubtitles());
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    [AvaloniaFact]
    public void OkBeforeThePreviewTimerTicksStillReturnsEveryLine()
    {
        // The preview timer only fires after 500 ms; FixedSubtitles used to stay empty until then
        // and the caller replaces the whole subtitle with it - wiping every line.
        var vm = ShowWindow();
        vm.MinGapMsOrFrames = 100;

        vm.OkCommand.Execute(null);

        Assert.Equal(new[] { "One", "Two", "Three" }, vm.FixedSubtitles.Select(p => p.Text));
        Assert.Equal(1900, vm.FixedSubtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(3000, vm.FixedSubtitles[1].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void OkUsesTheCurrentGapValueNotTheOneThePreviewLastSaw()
    {
        var vm = ShowWindow();
        vm.MinGapMsOrFrames = 100;
        Dispatcher.UIThread.RunJobs();

        vm.MinGapMsOrFrames = 500;
        vm.OkCommand.Execute(null);

        Assert.Equal(1500, vm.FixedSubtitles[0].EndTime.TotalMilliseconds);
    }
}
