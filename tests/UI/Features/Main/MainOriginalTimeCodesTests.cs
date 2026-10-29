using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// The original's own time codes: kept from the file, edited on their own in the timeline's original
/// row, or moved along with the translation while "Lock original to translation" is on.
/// </summary>
public class MainOriginalTimeCodesTests
{
    [AvaloniaFact]
    public void OneToOneImport_KeepsTheOriginalsOwnTimeCodes()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            ImportOriginal(vm, Original(("Original one", 100, 2100), ("Original two", 4000, 6000)));

            Assert.Equal(TimeSpan.FromMilliseconds(100), vm.Subtitles[0].OriginalStartTime);
            Assert.Equal(TimeSpan.FromMilliseconds(2100), vm.Subtitles[0].OriginalEndTime);
            Assert.False(vm.Subtitles[1].HasOwnOriginalTimeCodes);

            var original = vm.GetUpdateSubtitleOriginal();
            Assert.Equal(100, original.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(2100, original.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal(0, vm.Subtitles[0].StartTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void LockIsOnByDefault()
    {
        Assert.True(new SeGeneral().LockOriginalToTranslation);
    }

    [AvaloniaFact]
    public void Locked_RetimingTheTranslationMovesTheOriginalAlong()
    {
        using var _ = new SettingsScope("General.LockOriginalToTranslation");
        Se.Settings.General.LockOriginalToTranslation = true;
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            ImportOriginal(vm, Original(("Original one", 100, 2100), ("Original two", 4000, 6000)));

            vm.Subtitles[0].SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(1000));
            vm.Subtitles[0].EndTime = TimeSpan.FromMilliseconds(3500);
            vm.Subtitles[1].SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(5000));

            var original = vm.GetUpdateSubtitleOriginal();
            Assert.Equal(1100, original.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(3600, original.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal(5000, original.Paragraphs[1].StartTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Unlocked_RetimingTheTranslationLeavesAnOriginalWithOwnTimeCodes()
    {
        using var _ = new SettingsScope("General.LockOriginalToTranslation");
        Se.Settings.General.LockOriginalToTranslation = false;
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            ImportOriginal(vm, Original(("Original one", 100, 2100), ("Original two", 4000, 6000)));

            vm.Subtitles[0].SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(1000));
            vm.Subtitles[1].SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(5000));

            var original = vm.GetUpdateSubtitleOriginal();
            Assert.Equal(100, original.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(5000, original.Paragraphs[1].StartTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void DraggingTheOriginal_LeavesTheTranslation()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            ImportOriginal(vm, Original(("Original one", 0, 2000), ("Original two", 4000, 6000)));
            var hashBefore = vm.GetFastHashOriginal();

            var row = vm.Subtitles[0];
            vm.SetRowOriginalTimeCodes(row, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2500));

            Assert.Equal(0, row.StartTime.TotalMilliseconds);
            Assert.Equal(2000, row.EndTime.TotalMilliseconds);
            Assert.NotEqual(hashBefore, vm.GetFastHashOriginal());

            var original = vm.GetUpdateSubtitleOriginal();
            Assert.Equal(500, original.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(2500, original.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal(4000, original.Paragraphs[1].StartTime.TotalMilliseconds);

            // The undo snapshot is a copy of the rows, so it must carry the original's times.
            var snapshot = new SubtitleLineViewModel(row);
            Assert.Equal(row.OriginalStartTime, snapshot.OriginalStartTime);
            Assert.Equal(row.OriginalEndTime, snapshot.OriginalEndTime);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void DraggingAReferenceOnlyLine_RetimesItsOwnRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            var original = Original(("Original one", 0, 2000), ("Reference only", 2000, 4000), ("Original two", 4000, 6000));
            ImportOriginal(vm, original, ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, original));
            Assert.True(vm.IsShowingOriginalNonMatchingLines);

            var referenceRow = vm.Subtitles.Single(p => p.IsReferenceOnly);
            Assert.True(vm.HasOriginalLine(referenceRow));
            vm.SetRowOriginalTimeCodes(referenceRow, TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(3500));

            Assert.Equal(3500, referenceRow.EndTime.TotalMilliseconds);
            Assert.False(referenceRow.HasOwnOriginalTimeCodes);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReadOnly_RetimingTheTranslation_LeavesTheOriginalsTimeCodesForDrawing()
    {
        using var _ = new SettingsScope("General.LockOriginalToTranslation");
        Se.Settings.General.LockOriginalToTranslation = true;
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            var original = Original(("Original one", 100, 2100), ("Original two", 4000, 6000));
            ImportOriginal(vm, original, isReadOnly: true);

            vm.Subtitles[0].SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(1000));
            vm.Subtitles[1].SetStartTimeKeepDuration(TimeSpan.FromMilliseconds(5000));

            var timeCodes = vm.GetReadOnlyOriginalTimeCodes();
            Assert.NotNull(timeCodes);
            Assert.Equal((TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(2100)), timeCodes[original.Paragraphs[0].Id!.Value]);
            Assert.Equal((TimeSpan.FromMilliseconds(4000), TimeSpan.FromMilliseconds(6000)), timeCodes[original.Paragraphs[1].Id!.Value]);

            vm.IsOriginalReadOnly = false;
            Assert.Null(vm.GetReadOnlyOriginalTimeCodes());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Toggle_FlipsTheLock()
    {
        using var _ = new SettingsScope("General.LockOriginalToTranslation");
        Se.Settings.General.LockOriginalToTranslation = true;
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.ToggleLockOriginalToTranslationCommand.Execute(null);
            Assert.False(vm.IsOriginalLockedToTranslation);

            vm.ToggleLockOriginalToTranslationCommand.Execute(null);
            Assert.True(vm.IsOriginalLockedToTranslation);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static Subtitle Original(params (string Text, int StartMs, int EndMs)[] lines)
    {
        var subtitle = new Subtitle();
        foreach (var (text, startMs, endMs) in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, startMs, endMs));
        }

        return subtitle;
    }

    private static void ImportOriginal(MainViewModel vm, Subtitle original, ImportOriginalHelper.OriginalMatch? match = null, bool isReadOnly = false)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");
        method.Invoke(vm, new object?[] { 0, "original.srt", original, match, isReadOnly });
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
