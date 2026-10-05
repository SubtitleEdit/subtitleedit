using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.ApplyDurationLimits;

public class ApplyDurationLimitsViewModelTests : IDisposable
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
        // Too short (200 ms) with plenty of room before the next line.
        new SubtitleLineViewModel { Text = "One", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromMilliseconds(1200) },
        // Too long (10 s).
        new SubtitleLineViewModel { Text = "Two", StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(20) },
    };

    private ApplyDurationLimitsViewModel ShowWindow()
    {
        var vm = new ApplyDurationLimitsViewModel();
        var window = new ApplyDurationLimitsWindow(vm);
        _windows.Add(window);
        window.Show();
        vm.Initialize(MakeSubtitles(), new List<double>());
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    [AvaloniaFact]
    public async Task OkBeforeThePreviewTimerTicksStillAppliesTheLimits()
    {
        // The preview timer only fires after 250 ms; AllSubtitlesFixed used to stay empty until
        // then, so OK straight after opening silently did nothing.
        var vm = ShowWindow();
        vm.FixMinDurationMs = true;
        vm.MinDurationMsOrFrames = 1000;
        vm.FixMaxDurationMs = true;
        vm.MaxDurationMsOrFrames = 5000;

        await vm.OkCommand.ExecuteAsync(null);

        Assert.True(vm.OkPressed);
        Assert.Equal(new[] { "One", "Two" }, vm.AllSubtitlesFixed.Select(p => p.Text));
        Assert.Equal(1000, vm.AllSubtitlesFixed[0].Duration.TotalMilliseconds);
        Assert.Equal(5000, vm.AllSubtitlesFixed[1].Duration.TotalMilliseconds);
    }

    [AvaloniaFact]
    public async Task ShorteningStillWorksWhenTheUnusedMinimumIsHigherThanTheMaximum()
    {
        var vm = ShowWindow();
        vm.FixMinDurationMs = false;
        vm.MinDurationMsOrFrames = 3000;
        vm.FixMaxDurationMs = true;
        vm.MaxDurationMsOrFrames = 2000;

        await vm.OkCommand.ExecuteAsync(null);

        Assert.Equal(2000, vm.AllSubtitlesFixed[1].Duration.TotalMilliseconds);
    }
    [AvaloniaFact]
    public async Task OnlySelectedIdsAreFixedAndTheRestPassThroughUnchanged()
    {
        var vm = new ApplyDurationLimitsViewModel();
        var window = new ApplyDurationLimitsWindow(vm);
        _windows.Add(window);
        window.Show();
        var subtitles = MakeSubtitles();
        vm.Initialize(subtitles, new List<double>(), new HashSet<Guid> { subtitles[1].Id });
        Dispatcher.UIThread.RunJobs();
        vm.FixMinDurationMs = true;
        vm.MinDurationMsOrFrames = 1000;
        vm.FixMaxDurationMs = true;
        vm.MaxDurationMsOrFrames = 5000;

        await vm.OkCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.AllSubtitlesFixed.Count);
        Assert.Equal(200, vm.AllSubtitlesFixed[0].Duration.TotalMilliseconds);
        Assert.Equal(5000, vm.AllSubtitlesFixed[1].Duration.TotalMilliseconds);
    }

    [AvaloniaFact]
    public async Task FrameModeTakesTheLimitsInFramesAndSavesMilliseconds()
    {
        // Like Bridge gaps and Apply min gap: in frame mode the boxes hold frames.
        using var settings = new SettingsScope(
            "General.UseFrameMode",
            "General.CurrentFrameRate",
            "Tools.ApplyDurationLimitsMinDurationMs",
            "Tools.ApplyDurationLimitsMaxDurationMs");
        var frameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            // Both copies: OK saves the settings, which syncs Se's frame rate into libse.
            Configuration.Settings.General.CurrentFrameRate = 25;
            Se.Settings.General.CurrentFrameRate = 25;
            Se.Settings.General.UseFrameMode = true;
            Se.Settings.Tools.ApplyDurationLimitsMinDurationMs = 1200;
            Se.Settings.Tools.ApplyDurationLimitsMaxDurationMs = 8000;

            var vm = ShowWindow();
            Assert.Equal(Se.Language.Tools.ApplyDurationLimits.FixMinDurationFrames, vm.FixMinDurationLabel);
            Assert.Equal(Se.Language.Tools.ApplyDurationLimits.FixMaxDurationFrames, vm.FixMaxDurationLabel);
            Assert.Equal(30, vm.MinDurationMsOrFrames);
            Assert.Equal(200, vm.MaxDurationMsOrFrames);

            vm.FixMinDurationMs = true;
            vm.MinDurationMsOrFrames = 25;
            vm.FixMaxDurationMs = true;
            vm.MaxDurationMsOrFrames = 125;

            await vm.OkCommand.ExecuteAsync(null);

            Assert.Equal(1000, vm.AllSubtitlesFixed[0].Duration.TotalMilliseconds);
            Assert.Equal(5000, vm.AllSubtitlesFixed[1].Duration.TotalMilliseconds);
            Assert.Equal(1000, Se.Settings.Tools.ApplyDurationLimitsMinDurationMs);
            Assert.Equal(5000, Se.Settings.Tools.ApplyDurationLimitsMaxDurationMs);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = frameRate;
        }
    }

    [AvaloniaFact]
    public void FrameModeAt2997DoesNotFlagLinesExactlyAtTheLimit()
    {
        // At 29.97 fps two frames convert to 67 ms, but a line from frame 2 (67 ms) to frame 4
        // (133 ms) is 66 ms. Compared in frames it is exactly two frames - not too short.
        using var settings = new SettingsScope(
            "General.UseFrameMode",
            "General.CurrentFrameRate",
            "Tools.ApplyDurationLimitsMinDurationMs",
            "Tools.ApplyDurationLimitsMaxDurationMs");
        var frameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 29.97;
            Se.Settings.General.CurrentFrameRate = 29.97;
            Se.Settings.General.UseFrameMode = true;

            var vm = new ApplyDurationLimitsViewModel();
            vm.Initialize(new List<SubtitleLineViewModel>
            {
                // Exactly 2 frames (66 ms).
                new() { Text = "Two frames", StartTime = TimeSpan.FromMilliseconds(67), EndTime = TimeSpan.FromMilliseconds(133) },
                // Exactly 1 frame (34 ms) - 1 ms over the 33 ms one frame converts to.
                new() { Text = "One frame", StartTime = TimeSpan.FromMilliseconds(1001), EndTime = TimeSpan.FromMilliseconds(1035) },
                // 1 frame (33 ms) - really too short for a 2 frame minimum.
                new() { Text = "Short", StartTime = TimeSpan.FromMilliseconds(2002), EndTime = TimeSpan.FromMilliseconds(2035) },
            }, new List<double>());
            vm.FixMinDurationMs = true;
            vm.MinDurationMsOrFrames = 2;
            vm.FixMaxDurationMs = false;
            vm.MaxDurationMsOrFrames = 300;
            typeof(ApplyDurationLimitsViewModel)
                .GetMethod("BuildPreview", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(vm, null);

            Assert.Equal(new[] { "One frame", "Short" }, vm.Fixes.Select(f => f.SubtitleLine.Text));

            // A one frame maximum does not flag the line that is exactly one frame long.
            vm.FixMinDurationMs = false;
            vm.FixMaxDurationMs = true;
            vm.MinDurationMsOrFrames = 1;
            vm.MaxDurationMsOrFrames = 1;
            typeof(ApplyDurationLimitsViewModel)
                .GetMethod("BuildPreview", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(vm, null);

            Assert.Equal(new[] { "Two frames" }, vm.Fixes.Select(f => f.SubtitleLine.Text));
            vm.OnClosingCleanup();
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = frameRate;
        }
    }
}
